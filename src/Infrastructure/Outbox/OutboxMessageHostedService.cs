using Application.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Outbox;

internal sealed class OutboxMessageHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<OutboxMessageHostedService> logger)
    : IHostedService, IDisposable
{
    private readonly OutboxOptions _options = options.Value;
    private Timer? _timer;
    private bool _isProcessing;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Outbox Message Processor");

        _timer = new Timer(
            ProcessAsync,
            null,
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(_options.PollingIntervalMs));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping Outbox Message Processor");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private void ProcessAsync(object? state)
    {
        if (_isProcessing)
        {
            logger.LogDebug("Outbox message processing is already in progress, skipping this execution");
            return;
        }

        _isProcessing = true;

        try
        {
            using var scope = scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IOutboxMessageProcessor>();
            processor.ProcessAsync(_options.BatchSize, CancellationToken.None)
                .ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing outbox messages");
        }
        finally
        {
            _isProcessing = false;
        }
    }
}
