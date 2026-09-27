using System.Globalization;
using System.Reflection;
using SharedKernel.Extensions;

namespace Web.Api.Endpoints.Information;

internal sealed class GetVersion : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("info/version", HandleAsync)
            .WithName(nameof(GetVersion))
            .WithDescription("Get the current build version of the application")
            .Produces<GetVersionResponse>()
            .WithTags(Tags.Information)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Get version";
                operation.Description = "Get the current build version of the application";
                return Task.CompletedTask;
            });
    }

    private static IResult HandleAsync() =>
        Results.Ok(new GetVersionResponse(GetBuildDate(), GetBuildVersion()));

    private static string GetBuildDate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var attribute = assembly.GetFileCreationTimeUtc();
        return attribute.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
    }

    private static string GetBuildVersion() =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0";
}

internal sealed record GetVersionResponse(string BuildDate, string BuildVersion);
