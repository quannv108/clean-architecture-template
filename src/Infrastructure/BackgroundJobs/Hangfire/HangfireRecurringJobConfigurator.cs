using Hangfire;
using Infrastructure.Outbox;
using Infrastructure.AuditLogs;

namespace Infrastructure.BackgroundJobs.Hangfire;

internal class HangfireRecurringJobConfigurator(IRecurringJobManager recurringJobManager) : IRecurringJobConfigurator
{
    public void ConfigureRecurringJobs()
    {
        // Cleanup old processed outbox messages daily at 00:10
        recurringJobManager.AddOrUpdate<OutboxMessageCleanupJob>(
            "outbox-message-cleanup",
            job => job.ExecuteAsync(CancellationToken.None),
            Cron.Daily(hour: 0, minute: 10),
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });

        // Cleanup old audit logs monthly on the 10th at 02:00 UTC
        recurringJobManager.AddOrUpdate<DeleteOldAuditLogsBackgroundJob>(
            "delete-old-audit-logs",
            job => job.ExecuteAsync(CancellationToken.None),
            "0 2 10 * *", // Cron: 2 AM UTC on 10th of every month
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });
    }
}
