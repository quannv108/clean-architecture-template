namespace Web.Api.Extensions.OpenApi;

public static class WebApplicationExtensions
{
    public static IApplicationBuilder UseOpenApiWithUi(this WebApplication app)
    {
        // Map the OpenAPI endpoint with document name "v1"
        app.MapOpenApi("/openapi/{documentName}.json");

        // Configure SwaggerUI to work with native OpenAPI specification
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "API V1");
            options.RoutePrefix = "";
        });

        return app;
    }
}
