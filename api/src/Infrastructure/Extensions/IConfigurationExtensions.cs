using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions;

public static class IConfigurationExtensions
{
    public static string GetEnvironment(this IConfiguration configuration)
    {
        string? environment = configuration["ASPNETCORE_ENVIRONMENT"];
        if (string.IsNullOrEmpty(environment))
        {
            environment = configuration["ENVIRONMENT"];
        }

        return string.IsNullOrEmpty(environment) ? "Production" : environment;
    }
}
