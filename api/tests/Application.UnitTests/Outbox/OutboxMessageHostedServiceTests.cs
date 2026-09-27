using Application.Outbox;
using Infrastructure.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Application.UnitTests.Outbox;

public class OutboxMessageHostedServiceTests : IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOutboxMessageProcessor _processor;
    private readonly OutboxSignal _signal;
    private readonly CancellationTokenSource _applicationStoppingCts;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public OutboxMessageHostedServiceTests()
    {
        var serviceScope = Substitute.For<IServiceScope>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        _processor = Substitute.For<IOutboxMessageProcessor>();

        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _scopeFactory.CreateScope().Returns(serviceScope);
        serviceScope.ServiceProvider.Returns(serviceProvider);
        serviceProvider.GetService(typeof(IOutboxMessageProcessor)).Returns(_processor);

        _signal = new OutboxSignal();

        _applicationStoppingCts = new CancellationTokenSource();
        _applicationLifetime = Substitute.For<IHostApplicationLifetime>();
        _applicationLifetime.ApplicationStopping.Returns(_applicationStoppingCts.Token);
    }

    public void Dispose() => _applicationStoppingCts.Dispose();

    private OutboxMessageHostedService CreateSut(int minDelayMs = 100_000, int maxDelayMs = 600_000, int batchSize = 5)
    {
        var options = Microsoft.Extensions.Options.Options.Create(new OutboxOptions
        {
            MinPollingIntervalMs = minDelayMs,
            MaxPollingIntervalMs = maxDelayMs,
            BatchSize = batchSize
        });

        return new OutboxMessageHostedService(
            _scopeFactory,
            options,
            _signal,
            _applicationLifetime,
            NullLogger<OutboxMessageHostedService>.Instance);
    }

    private static ProcessedResult Result(int fetchedCount) =>
        new(fetchedCount, 0, 0, [], [], fetchedCount);

    [Fact]
    public async Task ExecuteAsync_ShouldDrainImmediately_WhileBatchesComeBackFull()
    {
        // Arrange - batch size 5: first two batches are full (5), third is partial (2) → stop draining
        var batchSize = 5;
        var callCount = 0;
        _processor.ProcessAsync(batchSize, Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callCount++;
                return callCount switch
                {
                    1 => Result(5),
                    2 => Result(5),
                    _ => Result(2)
                };
            });

        // Short min delay (inter-batch pacing) so the drain completes fast; long max so the test
        // only completes via the drain loop, not the idle fallback wait.
        using var sut = CreateSut(minDelayMs: 1, maxDelayMs: 600_000, batchSize: batchSize);

        // Act
        await sut.StartAsync(CancellationToken.None);
        // Give the background loop time to drain synchronously-resolved tasks
        await WaitUntil(() => callCount >= 3, TimeSpan.FromSeconds(5));
        await sut.StopAsync(CancellationToken.None);

        // Assert - drained until a non-full batch was returned, without waiting on the long delay
        callCount.ShouldBe(3);
        await _processor.Received(3).ProcessAsync(batchSize, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldKeepPolling_WhenIdle()
    {
        // Arrange - always empty: each idle cycle waits the (short) idle interval then polls again.
        const int batchSize = 5;
        var callCount = 0;
        _processor.ProcessAsync(batchSize, Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callCount++;
                return Task.FromResult(Result(0));
            });

        // Short idle interval so several fallback polls happen within the test window.
        using var sut = CreateSut(minDelayMs: 1_000, maxDelayMs: 50, batchSize: batchSize);

        // Act
        await sut.StartAsync(CancellationToken.None);
        await WaitUntil(() => callCount >= 3, TimeSpan.FromSeconds(5));
        await sut.StopAsync(CancellationToken.None);

        // Assert - the loop keeps polling at the fixed idle interval (no exponential growth).
        callCount.ShouldBeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldWakeImmediately_WhenSignalled_InsteadOfWaitingFullDelay()
    {
        // Arrange - long min delay; the processor returns empty so the loop would normally sleep
        // for a long time. A signal should wake it well before that delay elapses.
        const int batchSize = 5;
        var callCount = 0;
        var secondCallTcs = new TaskCompletionSource();
        _processor.ProcessAsync(batchSize, Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callCount++;
                if (callCount == 2)
                {
                    secondCallTcs.TrySetResult();
                }

                return Task.FromResult(Result(0));
            });

        using var sut = CreateSut(minDelayMs: 60_000, maxDelayMs: 60_000, batchSize: batchSize);

        // Act
        await sut.StartAsync(CancellationToken.None);
        await WaitUntil(() => callCount >= 1, TimeSpan.FromSeconds(5)); // first poll completes, enters wait
        _signal.Notify();

        var completed = await Task.WhenAny(secondCallTcs.Task, Task.Delay(TimeSpan.FromSeconds(5), CancellationToken.None));
        await sut.StopAsync(CancellationToken.None);

        // Assert
        completed.ShouldBe(secondCallTcs.Task, "signal should short-circuit the long idle delay");
    }

    [Fact]
    public async Task StopAsync_ShouldComplete_WhenParkedInIdleWait_OnLongInterval()
    {
        // Arrange - empty outbox so the loop parks in signal.WaitAsync on a very long idle interval.
        const int batchSize = 5;
        var callCount = 0;
        _processor.ProcessAsync(batchSize, Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callCount++;
                return Task.FromResult(Result(0));
            });

        using var sut = CreateSut(minDelayMs: 1_000, maxDelayMs: 600_000, batchSize: batchSize);

        await sut.StartAsync(CancellationToken.None);
        // Wait until the first (empty) poll completes → the loop is now parked in the 10-minute idle wait.
        await WaitUntil(() => callCount >= 1, TimeSpan.FromSeconds(5));

        // Act - stop while parked. If WaitAsync ignored cancellation this would hang for 10 minutes.
        var stop = sut.StopAsync(CancellationToken.None);
        var completed = await Task.WhenAny(stop, Task.Delay(TimeSpan.FromSeconds(5), CancellationToken.None));

        // Assert - StopAsync returns promptly, not blocked on the idle wait.
        completed.ShouldBe(stop, "StopAsync must not block on the idle WaitAsync");
        await stop;
    }

    private static async Task WaitUntil(Func<bool> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10);
        }
    }
}
