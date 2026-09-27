using Application.Abstractions.Communication.Email;
using Domain.Emails;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Emails;

internal sealed class SendTestEmail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/emails/test-send", HandleAsync)
            .WithName(nameof(SendTestEmail))
            .WithDescription("Send a test email. Only available outside production.")
            .Accepts<SendTestEmailRequest>("application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(Tags.Emails)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Send test email";
                operation.Description = "Send a test email. Only available outside production.";
                return Task.CompletedTask;
            });
    }

    private static async Task<IResult> HandleAsync(
        SendTestEmailRequest request,
        IHostEnvironment env,
        IEmailSender emailSender,
        CancellationToken cancellationToken)
    {
        if (env.IsProduction())
        {
            return Results.NotFound();
        }

        var createResult = EmailMessage.Create(
            request.To,
            request.Subject,
            request.Body,
            request.IsHtml,
            request.From);

        if (createResult.IsFailure)
        {
            return CustomResults.Problem(createResult);
        }

        var sendResult = await emailSender.SendAsync(createResult.Value, cancellationToken);

        return sendResult.Match(() => Results.Ok(), CustomResults.Problem);
    }
}

internal sealed record SendTestEmailRequest(
    string To,
    string Subject,
    string Body,
    bool IsHtml = true,
    string? From = null);
