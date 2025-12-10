using Web.Api.Infrastructure;

namespace Web.Api.Extensions.AuditLogs;

internal static class RouteHandlerBuilderExtensions
{
    public static RouteHandlerBuilder WithAuditLog(this RouteHandlerBuilder app, string actionName)
    {
        return app.WithMetadata(new AuditAttribute(actionName));
    }
}
