using Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.AuditLogs;

/// <summary>
/// Background job to clean up audit logs older than 3 years.
/// Runs monthly on the 10th at 2 AM UTC to remove old audit records.
/// </summary>
public sealed partial class DeleteOldAuditLogsBackgroundJob(
    ICommandHandler<DeleteOldAuditLogsCommand> commandHandler,
    ILogger<DeleteOldAuditLogsBackgroundJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        LogStarting();

        var command = new DeleteOldAuditLogsCommand();
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

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting audit log cleanup job")]
    private partial void LogStarting();

    [LoggerMessage(Level = LogLevel.Information, Message = "Audit log cleanup job completed successfully")]
    private partial void LogCompleted();

    [LoggerMessage(Level = LogLevel.Error, Message = "Audit log cleanup job failed: {Error}")]
    private partial void LogFailed(Error error);
}
