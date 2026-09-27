using Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Outbox;

/// <summary>
/// Background job to clean up old processed outbox messages.
/// Runs daily at 00:10 to remove messages that are processed and older than one month.
/// </summary>
public sealed partial class OutboxMessageCleanupJob(
    ICommandHandler<CleanupProcessedOutboxMessagesCommand, int> commandHandler,
    ILogger<OutboxMessageCleanupJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        LogStarting();

        var command = new CleanupProcessedOutboxMessagesCommand();
        var result = await commandHandler.Handle(command, cancellationToken);

        if (result.IsSuccess)
        {
            LogCompleted();
        }
        else
        {
            LogFailed(result.Error);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting outbox message cleanup job")]
    private partial void LogStarting();

    [LoggerMessage(Level = LogLevel.Information, Message = "Outbox message cleanup job completed successfully")]
    private partial void LogCompleted();

    [LoggerMessage(Level = LogLevel.Error, Message = "Outbox message cleanup job failed: {Error}")]
    private partial void LogFailed(Error error);
}
