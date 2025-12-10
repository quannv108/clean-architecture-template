using System.Reflection;
using Application;
using HealthChecks.UI.Client;
using Infrastructure;
using Infrastructure.Database.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using ServiceDefaults;
using Web.Api;
using Web.Api.Extensions;
using Web.Api.Extensions.AuditLogs;
using Web.Api.Extensions.Cors;
using Web.Api.Extensions.Hangfire;
using Web.Api.Extensions.OpenApi;
using Web.Api.Extensions.RateLimits;
using Web.Api.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext());

// the logger only for initialization steps
using var loggerFactory = LoggerFactory.Create(loggingBuilder =>
{
    loggingBuilder.AddConsole(); // Adds console logging
    loggingBuilder.AddDebug(); // Adds debug output logging
});

var logger = loggerFactory.CreateLogger<Program>();
try
{
    builder.AddServiceDefaults();

    builder.Services
        .AddApplication()
        .AddPresentation()
        .AddInfrastructure(builder.Configuration, logger, builder.Environment)
        .AddOpenApiWithAuth()
        .AddRateLimiters(builder.Configuration);

    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    });

    // Configure JSON options to handle enum serialization properly
    builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
    {
        options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

    builder.Services.AddCorsPolicy(builder.Configuration);
    builder.Services.AddScoped<RequestContextLoggingMiddleware>();
    builder.Services.AddRazorPages();
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    });

    WebApplication app = builder.Build();

    // Enable forwarded headers middleware for non-development environments
    // This updates HttpContext.Connection.RemoteIpAddress with the original client IP from proxies
    // Critical for audit logging in AWS ECS Fargate behind ALB, where requests pass through load balancer
    // Placed early in pipeline before other middleware to ensure IP is available throughout request
    if (!app.Environment.IsDevelopment())
    {
        app.UseForwardedHeaders();
    }

    if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
    {
        app.ApplyMigrations();
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseOpenApiWithUi();
        // keep razor testing page available on dev env only
        app.MapRazorPages();
        // Redirect root endpoint to OpenAPI UI in development
        app.MapGet("/", () => Results.Redirect("/index.html")).ExcludeFromDescription();
    }

    app.MapHealthChecks("health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    if (!app.Environment.IsEnvironment("Testing"))
    {
        app.UseHangfireDashboardWithBasicAuth();
        app.ConfigureRecurringJobs();
    }


    app.MapEndpoints(Assembly.GetExecutingAssembly());

    app.UseRateLimiter();

    app.UseRequestContextLogging();

    app.UseSerilogRequestLogging();

    app.UseExceptionHandler();

    app.UseAuthentication();

    // Apply the configured CORS policy to allow cross-origin requests.
    // This must be placed after UseRouting and before UseAuthorization.
    app.UseCorsPolicy();

    app.UseAuthorization();

    app.UseAuditLogging();

    // Enforce HTTPS with HSTS (HTTP Strict Transport Security) and redirection in non-development environments.
    // This tells browsers to always connect via HTTPS, preventing downgrade attacks.
    // For more details, see https://aka.ms/aspnetcore-hsts.
    // In development, allow HTTP to work properly (e.g., for Swagger UI on port 5000)
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
        app.UseHsts();
    }

    // REMARK: If you want to use Controllers, you'll need this.
    // app.MapControllers();

    await app.RunAsync();
}
#pragma warning disable S2139
catch (HostAbortedException ex)
{
    // Check if this is EF Core tools running (migrations, database updates, etc.)
    bool isEfToolsRunning = Environment.GetCommandLineArgs()
        .Any(arg => arg.Contains("ef", StringComparison.OrdinalIgnoreCase));

    if (isEfToolsRunning)
    {
        // Expected when EF Core tools abort the host after getting DbContext for migrations
        Log.Information("EF Core tools completed successfully");
    }
    else
    {
        // Unexpected host abortion during normal application execution
        Log.Fatal(ex, "Host was aborted unexpectedly");
        throw;
    }
}
catch (Exception ex)
#pragma warning restore S2139
{
    Log.Fatal(ex, "Unhandled exception");
    throw;
}
finally
{
#pragma warning disable S6966
#pragma warning disable CA1849
    Log.CloseAndFlush();
#pragma warning restore CA1849
#pragma warning restore S6966
}

// REMARK: Required for functional and integration tests to work.
namespace Web.Api
{
    public partial class Program;
}
