using System.Globalization;
using System.Reflection;
using SharedKernel.Extensions;

namespace Web.Api.Endpoints.Information;

internal sealed class GetVersion : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("info/version", () => Results.Ok(new
            {
                buildDate = GetBuildDate(),
                buildVersion = GetBuildVersion()
            }))
            .WithName(nameof(GetVersion))
            .WithDescription("Get the current build version of the application")
            .Produces(StatusCodes.Status200OK)
            .WithTags(Tags.Information)
            .AddOpenApiOperationTransformer((opperation, context, ct) =>
            {
                opperation.Summary = "Get version";
                opperation.Description = "Get version";
                return Task.CompletedTask;
            });
    }

    private static string GetBuildDate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var attribute = assembly.GetFileCreationTimeUtc();
        return attribute.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
    }

    private static string GetBuildVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0";
    }
}
