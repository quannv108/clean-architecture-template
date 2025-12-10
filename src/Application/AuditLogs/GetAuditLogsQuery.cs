using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Validation;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.AuditLogs;

public sealed record GetAuditLogsQuery : IQuery<GetAuditLogsResponse>
{
    public Guid? TenantId { get; init; }
    [RegularId] public Guid? UserId { get; init; } = null;
    public string? ActionName { get; init; }
    public DateTime? FromDateTime { get; init; }
    public DateTime? ToDateTime { get; init; }
    public int Take { get; init; } = 50;
}

public sealed record GetAuditLogsResponse(
    List<AuditLogResponse> AuditLogs,
    DateTime? LastActionDateTime,
    bool HasMore);

public sealed record AuditLogResponse(
    Guid Id,
    Guid UserId,
    string ActionName,
    DateTime ActionDateTime,
    string? Path,
    string? IpAddress,
    int? HttpResponseCode,
    string? AdditionalData);

internal sealed class GetAuditLogsQueryHandler(IReadOnlyApplicationDbContext context)
    : IQueryHandler<GetAuditLogsQuery, GetAuditLogsResponse>
{
    public async Task<Result<GetAuditLogsResponse>> Handle(
        GetAuditLogsQuery query,
        CancellationToken cancellationToken)
    {
        GetAuditLogsResponse response = await GetAuditLogsAsync(query, cancellationToken);

        return Result.Success(response);
    }

    private async Task<GetAuditLogsResponse> GetAuditLogsAsync(
        GetAuditLogsQuery query,
        CancellationToken cancellationToken = default)
    {
        var queryable = context.AuditLogs.AsNoTracking();

        // Apply filters
        if (query.UserId.HasValue)
        {
            queryable = queryable.Where(a => a.UserId == query.UserId.Value);
        }

        if (query.TenantId.HasValue)
        {
            queryable = queryable.Where(a => a.TenantId == query.TenantId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.ActionName))
        {
            queryable = queryable.Where(a => a.ActionName.Contains(query.ActionName));
        }

        if (query.FromDateTime.HasValue)
        {
            queryable = queryable.Where(a => a.ActionDateTime >= query.FromDateTime.Value);
        }

        if (query.ToDateTime.HasValue)
        {
            queryable = queryable.Where(a => a.ActionDateTime <= query.ToDateTime.Value);
        }

        // Order by ActionDateTime descending (newest first)
        queryable = queryable.OrderByDescending(a => a.ActionDateTime);

        // Take one more than requested to check if there are more records
        var auditLogs = await queryable
            .Take(query.Take + 1)
            .ToListAsync(cancellationToken);

        var hasMore = auditLogs.Count > query.Take;

        if (hasMore)
        {
            auditLogs.RemoveAt(auditLogs.Count - 1);
        }

        var lastActionDateTime = auditLogs.Count > 0
            ? auditLogs[^1].ActionDateTime
            : (DateTime?)null;

        var auditLogsResponse = auditLogs.Select(a => new AuditLogResponse(
                a.Id,
                a.UserId,
                a.ActionName,
                a.ActionDateTime,
                a.UrlPath,
                a.IpAddress,
                a.HttpResponseCode,
                a.AdditionalData))
            .ToList();

        return new GetAuditLogsResponse(auditLogsResponse, lastActionDateTime, hasMore);
    }
}
