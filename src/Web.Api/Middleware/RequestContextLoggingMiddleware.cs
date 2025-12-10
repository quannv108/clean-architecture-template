using System.Diagnostics;
using Application.Abstractions.Authentication;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace Web.Api.Middleware;

public class RequestContextLoggingMiddleware(IUserContext userContext) : IMiddleware
{
    private const string CorrelationIdHeaderName = "Correlation-Id";

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId = GetCorrelationId(context);

        // Add correlation ID to OpenTelemetry activity for distributed tracing
        Activity.Current?.SetBaggage("correlation-id", correlationId);
        Activity.Current?.SetTag("correlation.id", correlationId);

        var userId = userContext.UserId;
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            if (!userId.HasValue)
            {
                return next.Invoke(context);
            }

            using (LogContext.PushProperty("UserId", userId.Value))
            {
                return next.Invoke(context);
            }
        }
    }

    private static string GetCorrelationId(HttpContext context)
    {
        context.Request.Headers.TryGetValue(
            CorrelationIdHeaderName,
            out StringValues correlationId);

        return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
    }
}
