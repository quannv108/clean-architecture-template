using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Outbox;

public sealed record CleanupProcessedOutboxMessagesCommand : ICommand<int>;

internal sealed class CleanupProcessedOutboxMessagesCommandHandler(
    IApplicationDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    ILogger<CleanupProcessedOutboxMessagesCommandHandler> logger)
    : ICommandHandler<CleanupProcessedOutboxMessagesCommand, int>
{
    public async Task<Result<int>> Handle(CleanupProcessedOutboxMessagesCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var now = dateTimeProvider.UtcNow;
            var oneMonthAgo = now.AddMonths(-1);

            // Delete processed outbox messages that were last updated before last month
            var deletedCount = await dbContext.OutboxMessages
                .Where(om => om.Status == OutboxMessageStatus.Processed &&
                             om.ProcessedOnUtc != null &&
                             om.ProcessedOnUtc < oneMonthAgo)
                .ExecuteDeleteAsync(cancellationToken);

            logger.LogInformation(
                "Outbox message cleanup completed. Deleted {DeletedCount} processed outbox messages older than {CutoffDate}",
                deletedCount,
                oneMonthAgo);

            return Result.Success(deletedCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while cleaning up outbox messages");
            return Result.Failure<int>(Error.Failure("OutboxMessageCleanup.Failed",
                "Failed to cleanup processed outbox messages"));
        }
    }
}
