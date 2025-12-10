using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.AuditLogs;

public record DeleteOldAuditLogsCommand : ICommand
{
    /// <summary>
    /// Retention period for audit logs in days (3 years = 1095 days)
    /// </summary>
    public int RetentionDays { get; set; } = 3 * 365;

    /// <summary>
    /// Number of records to process in each batch to avoid table locks
    /// </summary>
    public int BatchSize { get; set; } = 10000;
}

internal sealed class DeleteOldAuditLogsCommandHandler(
    IApplicationDbContext dbContext,
    ILogger<DeleteOldAuditLogsCommandHandler> logger) : ICommandHandler<DeleteOldAuditLogsCommand>
{
    public async Task<Result> Handle(DeleteOldAuditLogsCommand command, CancellationToken cancellationToken)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-command.RetentionDays);
        var totalDeleted = 0;

        while (true)
        {
            // Delete batch of audit logs directly in database without fetching
            var deletedCount = await dbContext.AuditLogs
                .Where(al => al.ActionDateTime < cutoffDate)
                .Take(command.BatchSize)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedCount == 0)
            {
                break; // No more records to delete
            }

            totalDeleted += deletedCount;

            // Delay 100ms between batches to avoid prolonged table locks
            await Task.Delay(100, cancellationToken);
        }

        logger.LogInformation("Audit log cleanup completed. Total deleted: {TotalDeleted} records", totalDeleted);
        return Result.Success();
    }
}
