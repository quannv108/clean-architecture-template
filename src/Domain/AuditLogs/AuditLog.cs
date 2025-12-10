using SharedKernel;

namespace Domain.AuditLogs;

public sealed class AuditLog : Entity, ITenantEntity
{
    public Guid UserId { get; private set; }
    public Guid TenantId { get; private set; }
    public string ActionName { get; private set; } = string.Empty;
    public DateTime ActionDateTime { get; private set; }
    public string UrlPath { get; private set; } = string.Empty;
    public string? IpAddress { get; private set; }
    public int? HttpResponseCode { get; private set; }
    public string? AdditionalData { get; private set; }

    // Private constructor to enforce factory method usage
    private AuditLog() { }

    public static Result<AuditLog> Create(
        Guid userId,
        string actionName,
        DateTime actionDateTime,
        Uri urlPath,
        string? ipAddress = null,
        int? httpResponseCode = null,
        string? additionalData = null,
        Guid tenantId = default)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<AuditLog>(AuditLogErrors.UserIdRequired());
        }

        if (string.IsNullOrWhiteSpace(actionName))
        {
            return Result.Failure<AuditLog>(AuditLogErrors.ActionNameRequired());
        }

        if (actionDateTime == default)
        {
            return Result.Failure<AuditLog>(AuditLogErrors.ActionDateTimeRequired());
        }

        if (urlPath is null)
        {
            return Result.Failure<AuditLog>(AuditLogErrors.UrlPathRequired());
        }

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            ActionName = actionName.Trim(),
            ActionDateTime = actionDateTime,
            UrlPath = urlPath.ToString(),
            IpAddress = ipAddress?.Trim(),
            HttpResponseCode = httpResponseCode,
            AdditionalData = additionalData?.Trim()
        };

        return Result.Success(auditLog);
    }
}
