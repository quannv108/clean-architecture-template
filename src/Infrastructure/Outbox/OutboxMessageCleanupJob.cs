using Application.Abstractions.Messaging;
using Application.Outbox;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Outbox;

/// <summary>
/// Background job to clean up old processed outbox messages.
/// Runs daily at 00:10 to remove messages that are processed and older than one month.
/// </summary>
internal sealed class OutboxMessageCleanupJob(
    ICommandHandler<CleanupProcessedOutboxMessagesCommand, int> commandHandler,
    ILogger<OutboxMessageCleanupJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting outbox message cleanup job");

        var command = new CleanupProcessedOutboxMessagesCommand();
        var result = await commandHandler.Handle(command, cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("Outbox message cleanup job completed successfully");
        }
        else
        {
            logger.LogError("Outbox message cleanup job failed: {Error}", result.Error);
        }
    }
}
