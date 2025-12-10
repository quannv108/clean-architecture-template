using Application.Abstractions.Messaging;
using Application.AuditLogs;
using Microsoft.Extensions.Logging;

namespace Infrastructure.AuditLogs;

/// <summary>
/// Background job to clean up audit logs older than 3 years.
/// Runs monthly on the 10th at 2 AM UTC to remove old audit records.
/// </summary>
internal sealed class DeleteOldAuditLogsBackgroundJob(
    ICommandHandler<DeleteOldAuditLogsCommand> commandHandler,
    ILogger<DeleteOldAuditLogsBackgroundJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting audit log cleanup job");

        var command = new DeleteOldAuditLogsCommand();
        var result = await commandHandler.Handle(command, cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("Audit log cleanup job completed successfully");
        }
        else
        {
            logger.LogError("Audit log cleanup job failed: {Error}", result.Error);
        }
    }
}
