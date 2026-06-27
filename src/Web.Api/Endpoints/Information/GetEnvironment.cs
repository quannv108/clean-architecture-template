using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Web.Api.Endpoints.Information;

internal sealed class GetEnvironment : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("info/environment", HandleAsync)
            .WithName(nameof(GetEnvironment))
            .WithDescription("Get the current environment information")
            .Produces<GetEnvironmentResponse>()
            .WithTags(Tags.Information)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Get environment";
                operation.Description = "Get the current environment information";
                return Task.CompletedTask;
            });
    }

    private static IResult HandleAsync(IHostEnvironment env) =>
        Results.Ok(new GetEnvironmentResponse(
            env.EnvironmentName,
            GetRuntimeInfo(),
            GetMemoryInfo()));

    private static RuntimeInfo GetRuntimeInfo() =>
        new(
            RuntimeInformation.OSDescription,
            RuntimeInformation.OSArchitecture.ToString(),
            Environment.Version.ToString(),
            Environment.MachineName,
            Environment.ProcessorCount,
            TimeZoneInfo.Local.Id);

    private static MemoryInfo GetMemoryInfo()
    {
        var process = Process.GetCurrentProcess();
        return new(
            $"{process.WorkingSet64 / (1024 * 1024)} MB",
            $"{process.PrivateMemorySize64 / (1024 * 1024)} MB",
            $"{GC.GetTotalMemory(false) / (1024 * 1024)} MB");
    }
}

internal sealed record GetEnvironmentResponse(string Environment, RuntimeInfo Runtime, MemoryInfo MemoryInfo);

internal sealed record RuntimeInfo(
    string Platform,
    string Architecture,
    string DotNetVersion,
    string MachineName,
    int ProcessorCount,
    string Timezone);

internal sealed record MemoryInfo(
    string WorkingSetMemory,
    string PrivateMemory,
    string ManagedMemory);
