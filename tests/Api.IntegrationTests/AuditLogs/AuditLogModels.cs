namespace Api.IntegrationTests.AuditLogs;

public record AuditLogListResponse(
    List<AuditLogResponse> AuditLogs,
    DateTime? LastActionDateTime,
    bool HasMore);

public record AuditLogResponse(
    Guid Id,
    Guid UserId,
    string ActionName,
    DateTime ActionDateTime,
    Uri UrlPath,
    string? IpAddress,
    int? HttpResponseCode,
    string? AdditionalData);
