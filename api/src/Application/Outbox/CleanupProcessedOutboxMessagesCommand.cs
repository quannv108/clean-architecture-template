using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Outbox;

public sealed record CleanupProcessedOutboxMessagesCommand : ICommand<int>;

internal sealed partial class CleanupProcessedOutboxMessagesCommandHandler(
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

            LogCleanupCompleted(deletedCount, oneMonthAgo);

            return Result.Success(deletedCount);
        }
        catch (Exception ex)
        {
            LogCleanupFailed(ex);
            return Result.Failure<int>(Error.Failure("OutboxMessageCleanup.Failed",
                "Failed to cleanup processed outbox messages"));
        }
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Outbox message cleanup completed. Deleted {DeletedCount} processed outbox messages older than {CutoffDate}")]
    private partial void LogCleanupCompleted(int deletedCount, DateTime cutoffDate);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error occurred while cleaning up outbox messages")]
    private partial void LogCleanupFailed(Exception exception);
}
