using System.Threading.RateLimiting;
using Infrastructure.Extensions;

namespace Web.Api.Extensions.RateLimits;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRateLimiters(this IServiceCollection services, IConfiguration configuration)
    {
        var isTesting = configuration.GetEnvironment() == "Testing";

        services.AddRateLimiter(options =>
        {
            // Configure rate limiter to return 429 Too Many Requests instead of default 503
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Disable global rate limiter in Testing environment to prevent test failures
            if (!isTesting)
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.GetClientIpAddress() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 30,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }));
            }

            options.AddPolicy(RateLimitPolicyNameConstants.LimitPerUser, context =>
            {
                var username = "anonymous";
                if (context.User.Identity?.IsAuthenticated is true)
                {
                    username = context.User.Identity.Name;
                }

                int defaultPermitLimit = 30;
                if (configuration.GetEnvironment() == "Testing")
                {
                    // For testing, we can increase the limit to avoid rate limiting issues
                    defaultPermitLimit = 200;
                }

                return RateLimitPartition.GetSlidingWindowLimiter(username,
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = defaultPermitLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 1
                    });
            });
        });

        return services;
    }
}
