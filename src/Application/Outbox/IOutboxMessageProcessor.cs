using Application.Abstractions.Data;
using Application.Abstractions.DomainEvents;
using Application.Abstractions.Locking;
using Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Outbox;

/// <summary>
/// Processes pending outbox messages by deserializing and dispatching domain events.
/// </summary>
public interface IOutboxMessageProcessor
{
    /// <summary>
    /// Processes a batch of pending outbox messages.
    /// </summary>
    /// <param name="batchSize">Maximum number of messages to process in this batch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing counts of processed, failed, and skipped messages.</returns>
    Task<ProcessedResult> ProcessAsync(int batchSize, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of processing a batch of outbox messages.
/// </summary>
/// <param name="ProcessedCount">Number of successfully processed messages.</param>
/// <param name="FailedCount">Number of messages that failed to process.</param>
/// <param name="SkipCount">Number of messages skipped (already being processed by another instance).</param>
/// <param name="SucceedLogs">List of successfully processed event types.</param>
/// <param name="FailedLogs">List of failed event types with error messages.</param>
public sealed record ProcessedResult(
    int ProcessedCount,
    int FailedCount,
    int SkipCount,
    List<string> SucceedLogs,
    List<string> FailedLogs);

internal sealed class OutboxMessageProcessor(
    IServiceScopeFactory serviceScopeFactory,
    IDistributedLockProvider lockProvider,
    ILogger<OutboxMessageProcessor> logger) : IOutboxMessageProcessor
{
    private static readonly ProcessedResult EmptyProcessedResult = new(0, 0, 0,
        [], []);

    public async Task<ProcessedResult> ProcessAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var domainEventsDispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventsDispatcher>();

        // Query for unprocessed outbox messages
        var outboxMessages = await dbContext.OutboxMessages
            .AsNoTracking()
            .Where(m => m.Status == OutboxMessageStatus.Pending &&
                        m.ProcessedOnUtc == null &&
                        m.OccurredOnUtc <= DateTime.UtcNow)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        if (outboxMessages.Count == 0)
        {
            logger.LogDebug("No outbox messages to process");
            return EmptyProcessedResult;
        }

        logger.LogInformation("Processing {Count} outbox messages", outboxMessages.Count);

        var successCount = 0;
        var failureCount = 0;
        var skipCount = 0;
        var processed = new List<string>();
        var failed = new List<string>();
        foreach (var outboxMessage in outboxMessages)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Cancellation requested, stopping outbox processing in the middle of batch");
                break;
            }

            // Use distributed lock to prevent duplicate processing across instances
            var lockName = $"outbox:message:{outboxMessage.Id}";
            await using var lockHandle = await lockProvider
                .CreateLock(lockName)
                .TryAcquireAsync(TimeSpan.Zero, cancellationToken); // Try immediately, don't wait

            if (lockHandle is null)
            {
                // Another instance is already processing this message
                skipCount++;
                logger.LogDebug(
                    "Skipping outbox message {Id} because it is already being processed by another instance",
                    outboxMessage.Id);
                continue;
            }

            try
            {
                // Deserialize the event
                var domainEvent = DeserializeDomainEvent(outboxMessage);

                if (domainEvent == null)
                {
                    failureCount++;
                    failed.Add(outboxMessage.Type + "(deserialize failed)");
                    logger.LogWarning("Failed to deserialize domain event of type {EventType}", outboxMessage.Type);
                    continue;
                }

                // Double-check status after acquiring lock (defense in depth)
                var currentStatus = await dbContext.OutboxMessages
                    .AsNoTracking()
                    .Where(x => x.Id == outboxMessage.Id)
                    .Select(x => x.Status)
                    .SingleOrDefaultAsync(CancellationToken.None);
                if (currentStatus != OutboxMessageStatus.Pending)
                {
                    skipCount++;
                    logger.LogDebug("Skipping outbox message {Id} because status changed to {Status}",
                        outboxMessage.Id, currentStatus);
                    continue;
                }

                // Mark as processing
                var timer = System.Diagnostics.Stopwatch.StartNew();
                outboxMessage.MarkAsProcessing(Environment.MachineName);
                dbContext.OutboxMessages.Update(outboxMessage);
                await dbContext.SaveChangesAsync(CancellationToken.None);

                // Dispatch the event
                await domainEventsDispatcher.DispatchAsync(new[] { domainEvent }, CancellationToken.None);

                // Update the outbox message as processed
                outboxMessage.MarkAsProcessed(DateTime.UtcNow, Environment.MachineName);
                dbContext.OutboxMessages.Update(outboxMessage);
                await dbContext.SaveChangesAsync(CancellationToken.None);
                timer.Stop();

                logger.LogDebug(
                    "Successfully processed outbox message {MessageId} of type {EventType} Take {ElapsedMilliseconds} ms",
                    outboxMessage.Id, outboxMessage.Type, timer.ElapsedMilliseconds);
                successCount++;
                processed.Add(outboxMessage.Type);
            }
            catch (Exception ex)
            {
                failureCount++;
                failed.Add(outboxMessage.Type + $"(Ex:{ex.Message}");
                logger.LogError(ex, "Error processing outbox message {MessageId} of type {EventType}",
                    outboxMessage.Id, outboxMessage.Type);

                // Update the outbox message with error information
                outboxMessage.SetError(ex.ToString());
                dbContext.OutboxMessages.Update(outboxMessage);
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }
            // Lock is automatically released here when lockHandle is disposed
        }

        logger.LogInformation(
            "Outbox processing completed. Success: {SuccessCount}, Failures: {FailureCount}, Skip {SkipCount}",
            successCount, failureCount, skipCount);
        return new ProcessedResult(successCount, failureCount, skipCount, processed, failed);
    }

    private IDomainEvent? DeserializeDomainEvent(OutboxMessage outboxMessage)
    {
        var domainEvent = outboxMessage.GetDomainEvent();
        if (domainEvent.IsFailure)
        {
            logger.LogError("Error deserializing domain event of type {EventType}", outboxMessage.Type);
            return null;
        }

        return domainEvent.Value;
    }
}
