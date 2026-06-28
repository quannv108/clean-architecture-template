using Application.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Outbox;

internal sealed class OutboxMessageHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxOptions> options,
    OutboxSignal signal,
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
        var idleInterval = TimeSpan.FromMilliseconds(_options.MaxPollingIntervalMs);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DrainAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing outbox messages");
                }

                // Idle until a new message signals us, or the fallback interval elapses.
                await signal.WaitAsync(idleInterval, stoppingToken);
            }
        }
        catch (OperationCanceledException e) when (stoppingToken.IsCancellationRequested)
        {
            // Expected during shutdown
            logger.LogInformation(message: $"{nameof(OutboxMessageHostedService)} stopping due to {nameof(OperationCanceledException)}", exception: e);
        }
    }

    /// <summary>
    /// Processes batches while they come back full (rows fetched == batch size), draining a backlog
    /// instead of waiting for the next idle tick. The first batch runs immediately (so a signalled
    /// wake stays instant); subsequent batches are paced by <see cref="OutboxOptions.MinPollingIntervalMs"/>
    /// to avoid hammering the database. Stops on the first partial or empty batch.
    /// </summary>
    private async Task DrainAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_options.MinPollingIntervalMs));

        while (true)
        {
            int fetched;
            using (var scope = scopeFactory.CreateScope())
            {
                var processor = scope.ServiceProvider.GetRequiredService<IOutboxMessageProcessor>();
                var result = await processor.ProcessAsync(_options.BatchSize, stoppingToken);
                fetched = result.FetchedCount;
            }

            if (fetched < _options.BatchSize)
            {
                break;
            }

            // Backlog continues: pace the next batch poll. Scope above is already disposed so we
            // don't hold a DbContext open during the wait. Exits if the app is stopping.
            if (!await timer.WaitForNextTickAsync(stoppingToken))
            {
                break;
            }
        }
    }
}
