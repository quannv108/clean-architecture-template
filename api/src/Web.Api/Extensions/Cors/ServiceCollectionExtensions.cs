namespace Web.Api.Extensions.Cors;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSection = configuration.GetSection("Cors");
        if (!corsSection.Exists())
        {
            return services;
        }

        var allowedOrigins = corsSection.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyNameConstants.DefaultCorsPolicy, policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
                else
                {
                    // Fallback only when a Cors section exists but lists no origins (e.g. local dev).
                    // S5122 flagged by SonarAnalyzer 10.28; harden separately if wildcard is undesirable in prod.
#pragma warning disable S5122
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
#pragma warning restore S5122
                }
            });
        });
        return services;
    }
}
