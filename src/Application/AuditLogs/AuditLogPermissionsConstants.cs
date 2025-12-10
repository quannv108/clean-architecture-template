namespace Application.AuditLogs;

public static class AuditLogPermissionsConstants
{
    public const string AuditLogsRead = "auditlogs:read";

    public static readonly string[] AllPermissions =
    [
        AuditLogsRead
    ];
}
