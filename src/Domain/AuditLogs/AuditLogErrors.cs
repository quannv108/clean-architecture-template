using SharedKernel;

namespace Domain.AuditLogs;

public static class AuditLogErrors
{
    public static Error UserIdRequired() => Error.Validation(
        "AuditLogs.UserIdRequired",
        "User ID is required for audit log entry");

    public static Error ActionNameRequired() => Error.Validation(
        "AuditLogs.ActionNameRequired",
        "Action name is required for audit log entry");

    public static Error ActionDateTimeRequired() => Error.Validation(
        "AuditLogs.ActionDateTimeRequired",
        "Action date time is required for audit log entry");

    public static Error UrlPathRequired() => Error.Validation(
        "AuditLogs.UrlPathRequired",
        "URL path is required for audit log entry");

    public static Error InvalidUrlPathFormat() => Error.Validation(
        "AuditLogs.InvalidUrlPathFormat",
        "URL path must be a valid URI format");

    public static Error NotFound(Guid auditLogId) => Error.NotFound(
        "AuditLogs.NotFound",
        $"The audit log with ID '{auditLogId}' was not found");
}
