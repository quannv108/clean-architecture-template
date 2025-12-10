using Microsoft.AspNetCore.Diagnostics;

namespace Web.Api.Infrastructure;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred");

        var problemDetails = new
        {
            status = StatusCodes.Status500InternalServerError,
            type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            title = "Server failure"
        };

        httpContext.Response.StatusCode = problemDetails.status;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
