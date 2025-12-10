using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Web.Api.Extensions.OpenApi;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddOpenApiWithAuth(this IServiceCollection services)
    {
        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });

        return services;
    }
}

internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        // Set API version info
        document.Info.Version = "1.0";
        document.Info.Title = "Clean Architecture API";
        document.Info.Description = ".NET 9 Clean Architecture API with CQRS, DDD, and Vertical Slice Architecture";

        var securityScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT",
            Description = "Enter your JWT token in this field"
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] = securityScheme;

        var securityRequirement = new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = JwtBearerDefaults.AuthenticationScheme
                    }
                },
                Array.Empty<string>()
            }
        };

        document.SecurityRequirements.Add(securityRequirement);

        return Task.CompletedTask;
    }
}
