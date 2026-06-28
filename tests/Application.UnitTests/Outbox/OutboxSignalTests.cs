using Infrastructure.Outbox;

namespace Application.UnitTests.Outbox;

public class OutboxSignalTests
{
    [Fact]
    public async Task WaitAsync_ShouldReturnBeforeTimeout_WhenSignalled()
    {
        // Arrange
        var signal = new OutboxSignal();
        var timeout = TimeSpan.FromSeconds(30);

        // Act
        var waitTask = signal.WaitAsync(timeout, CancellationToken.None);
        signal.Notify();
        var completed = await Task.WhenAny(waitTask, Task.Delay(TimeSpan.FromSeconds(5)));

        // Assert
        completed.ShouldBe(waitTask, "WaitAsync should short-circuit the timeout once Notify() is called");
        await waitTask; // should already be complete; awaiting surfaces any exception
    }

    [Fact]
    public async Task WaitAsync_ShouldReturnAfterTimeout_WhenNotSignalled()
    {
        // Arrange
        var signal = new OutboxSignal();
        var timeout = TimeSpan.FromMilliseconds(100);

        // Act
        var start = DateTime.UtcNow;
        await signal.WaitAsync(timeout, CancellationToken.None);
        var elapsed = DateTime.UtcNow - start;

        // Assert
        elapsed.ShouldBeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(80));
    }

    [Fact]
    public void Notify_ShouldCoalesceBurstsIntoSingleSignal()
    {
        // Arrange
        var signal = new OutboxSignal();

        // Act - burst of notifications before anyone waits
        signal.Notify();
        signal.Notify();
        signal.Notify();

        // Assert - does not throw, capacity-1 channel drops extra writes (FullMode = DropWrite)
        Should.NotThrow(() => signal.Notify());
    }

    [Fact]
    public async Task WaitAsync_ShouldThrow_WhenCancellationTokenCancelled()
    {
        // Arrange
        var signal = new OutboxSignal();
        using var cts = new CancellationTokenSource();

        // Act
        var waitTask = signal.WaitAsync(TimeSpan.FromSeconds(30), cts.Token);
        await cts.CancelAsync();

        // Assert
        await Should.ThrowAsync<OperationCanceledException>(async () => await waitTask);
    }
}
