using Application.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Outbox;

internal sealed class OutboxMessageHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxOptions> options,
    IHostApplicationLifetime applicationLifetime,
    ILogger<OutboxMessageHostedService> logger)
    : IHostedService, IDisposable
{
    private readonly OutboxOptions _options = options.Value;
    private CancellationTokenSource? _cts;
    private Task? _executingTask;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Outbox Message Processor");

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Stop immediately when the application is stopping to avoid querying disposed DbContext
        applicationLifetime.ApplicationStopping.Register(() =>
        {
            logger.LogInformation("Stopping Outbox Message Processor");
            _cts?.Cancel();
        });

        _executingTask = ExecuteAsync(_cts.Token);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cts is not null && !_cts.IsCancellationRequested)
        {
            await _cts.CancelAsync();
        }

        if (_executingTask is not null)
        {
            try
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
        }

        logger.LogInformation("Outbox Message Processor stopped");
    }

    public void Dispose()
    {
        _cts?.Dispose();
    }

    private async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_options.PollingIntervalMs));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var processor = scope.ServiceProvider.GetRequiredService<IOutboxMessageProcessor>();
                    await processor.ProcessAsync(_options.BatchSize, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing outbox messages");
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Expected during shutdown
        }
    }
}
