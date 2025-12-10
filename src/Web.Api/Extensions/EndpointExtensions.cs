using System.Reflection;
using Serilog;
using Web.Api.Endpoints;

namespace Web.Api.Extensions;

public static class EndpointExtensions
{
    public static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        Assembly assembly,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndpoint> endpoints = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => (IEndpoint)Activator.CreateInstance(type)!)
            .ToArray();

        // Create a versioned route group (v1)
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        RouteGroupBuilder v1Group = app.MapGroup("api/v1")
            .WithApiVersionSet(versionSet)
            .WithOpenApi();

        IEndpointRouteBuilder builder = routeGroupBuilder ?? v1Group;

        foreach (IEndpoint endpoint in endpoints)
        {
            Log.Information("Mapping endpoint: {Endpoint}", endpoint.GetType().FullName);
            endpoint.MapEndpoint(builder);
        }

        // CRUD endpoints add here

        return app;
    }

    public static RouteHandlerBuilder RequirePermission(this RouteHandlerBuilder app, string permission)
    {
        return app.RequireAuthorization(permission);
    }
}
