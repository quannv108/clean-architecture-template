using Application.Abstractions.Messaging;
using Application.AuditLogs;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.AuditLogs;

internal sealed class GetAuditLogs : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/audit-logs", async (
                Guid? tenantId,
                Guid? userId,
                string? actionName,
                DateTime? fromDateTime,
                DateTime? toDateTime,
                int take,
                IQueryHandler<GetAuditLogsQuery, GetAuditLogsResponse> queryHandler,
                CancellationToken cancellationToken) =>
            {
                var query = new GetAuditLogsQuery
                {
                    TenantId = tenantId,
                    UserId = userId,
                    ActionName = actionName,
                    FromDateTime = fromDateTime,
                    ToDateTime = toDateTime,
                    Take = take <= 0 ? 50 : Math.Min(take, 100) // Default 50, max 100
                };

                Result<GetAuditLogsResponse> result = await queryHandler.Handle(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .WithName(nameof(GetAuditLogs))
            .WithDescription("Get audit logs")
            .Produces<GetAuditLogsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequirePermission(AuditLogPermissionsConstants.AuditLogsRead)
            .RequireAuthorization()
            .WithTags(Tags.AuditLogs);
    }
}
