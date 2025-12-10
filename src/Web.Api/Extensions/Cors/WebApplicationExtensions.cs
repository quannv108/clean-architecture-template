namespace Web.Api.Extensions.Cors;

public static class WebApplicationExtensions
{
    public static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app)
    {
        app.UseCors(CorsPolicyNameConstants.DefaultCorsPolicy);
        return app;
    }
}
