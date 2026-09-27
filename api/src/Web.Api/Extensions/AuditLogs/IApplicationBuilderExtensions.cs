using Web.Api.Middleware;

namespace Web.Api.Extensions.AuditLogs;

internal static class IApplicationBuilderExtensions
{
    public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<AuditLoggingMiddleware>();

        return app;
    }
}
