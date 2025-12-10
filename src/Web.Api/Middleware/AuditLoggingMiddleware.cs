using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.AuditLogs;
using Domain;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Middleware;

internal sealed class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory,
        ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if the endpoint has audit metadata
        var auditMetadata = context.GetEndpoint()?.Metadata.GetMetadata<AuditAttribute>();
        if (auditMetadata is null)
        {
            await _next(context);
            return;
        }

        // Capture initial request data
        var requestData = CaptureRequestData(context, auditMetadata);

        // Execute the request
        await _next(context);

        // Send audit log command (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await SendAuditLogAsync(requestData, context.Response.StatusCode);
            }
            catch (Exception ex)
            {
                // Fallback logging in case of failure
                _logger.LogError(ex, "Failed to log audit data for request to {Path} with action {ActionName}",
                    context.Request.Path, auditMetadata.ActionName);
            }
        });
    }

    private static AuditRequestData CaptureRequestData(HttpContext context, AuditAttribute auditMetadata)
    {
        var userContext = context.RequestServices.GetService<IUserContext>();

        // For authenticated requests, use the user ID
        if (userContext?.UserId is not null)
        {
            return new AuditRequestData(
                userContext.UserId.Value,
                auditMetadata.ActionName,
                DateTime.UtcNow,
                new Uri(context.Request.Path + context.Request.QueryString, UriKind.Relative),
                context.GetClientIpAddress(),
                userContext.TenantId);
        }

        // For unauthenticated requests (like registration), use the anonymous system user
        return new AuditRequestData(
            SystemConstants.AnonymousUserId,
            auditMetadata.ActionName,
            DateTime.UtcNow,
            new Uri(context.Request.Path + context.Request.QueryString, UriKind.Relative),
            context.GetClientIpAddress(),
            SystemConstants.SystemTenantId);
    }

    private async Task SendAuditLogAsync(AuditRequestData requestData, int statusCode)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var commandHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateAuditLogCommand, Guid>>();

        var command = new CreateAuditLogCommand
        {
            UserId = requestData.UserId,
            ActionName = requestData.ActionName,
            ActionDateTime = requestData.ActionDateTime,
            UrlPath = requestData.UrlPath,
            IpAddress = requestData.IpAddress,
            HttpResponseCode = statusCode,
            TenantId = requestData.TenantId ?? SystemConstants.UnknowTenantId
        };

        await commandHandler.Handle(command, CancellationToken.None);
    }

    private sealed record AuditRequestData(
        Guid UserId,
        string ActionName,
        DateTime ActionDateTime,
        Uri UrlPath,
        string? IpAddress,
        Guid? TenantId);
}
