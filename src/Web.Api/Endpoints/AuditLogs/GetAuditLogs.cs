using Application.Abstractions.Messaging;
using Application.AuditLogs;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.AuditLogs;

internal sealed class GetAuditLogs : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/audit-logs", HandleAsync)
            .WithName(nameof(GetAuditLogs))
            .WithDescription("Get audit logs")
            .Produces<GetAuditLogsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(Tags.AuditLogs)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Get audit logs";
                operation.Description = "Get audit logs";
                return Task.CompletedTask;
            });
    }

    private static async Task<IResult> HandleAsync(
        Guid? tenantId,
        Guid? userId,
        string? actionName,
        DateTime? fromDateTime,
        DateTime? toDateTime,
        int? take,
        IQueryHandler<GetAuditLogsQuery, GetAuditLogsResponse> queryHandler,
        CancellationToken cancellationToken)
    {
        var query = new GetAuditLogsQuery
        {
            TenantId = tenantId,
            UserId = userId,
            ActionName = actionName,
            FromDateTime = fromDateTime,
            ToDateTime = toDateTime,
            Take = take <= 0 ? 50 : Math.Min(take ?? 50, 100) // Default 50, max 100
        };

        Result<GetAuditLogsResponse> result = await queryHandler.Handle(query, cancellationToken);

        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
