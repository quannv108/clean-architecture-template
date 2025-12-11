using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Web.Api.Endpoints.Information;

internal sealed class GetEnvironment : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("info/environment", (IHostEnvironment environment) => Results.Ok(new
            {
                environment = environment.EnvironmentName,
                runtime = GetRuntimeInfo(),
                memoryInfo = GetMemoryInfo(),
            }))
            .WithName(nameof(GetEnvironment))
            .WithDescription("Get the current environment information")
            .Produces(StatusCodes.Status200OK)
            .WithTags(Tags.Information)
            .AddOpenApiOperationTransformer((opperation, context, ct) =>
            {
                opperation.Summary = "Get the current environment information";
                opperation.Description = "Get the current environment information";
                return Task.CompletedTask;
            });
    }

    private static object GetRuntimeInfo()
    {
        return new
        {
            platform = RuntimeInformation.OSDescription,
            architecture = RuntimeInformation.OSArchitecture.ToString(),
            dotNetVersion = Environment.Version.ToString(),
            machineName = Environment.MachineName,
            processorCount = Environment.ProcessorCount,
            timezone = TimeZoneInfo.Local.Id
        };
    }

    private static object GetMemoryInfo()
    {
        var process = Process.GetCurrentProcess();

        return new
        {
            // Working set (physical memory) - RAM currently used
            workingSetMemory = $"{process.WorkingSet64 / (1024 * 1024)} MB",
            // Private memory (process-specific allocated memory)
            privateMemory = $"{process.PrivateMemorySize64 / (1024 * 1024)} MB",
            // .NET managed heap size
            managedMemory = $"{GC.GetTotalMemory(false) / (1024 * 1024)} MB",
        };
    }
}
