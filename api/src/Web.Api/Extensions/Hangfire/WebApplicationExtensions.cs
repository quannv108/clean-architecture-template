using Hangfire;
using Infrastructure.BackgroundJobs;

namespace Web.Api.Extensions.Hangfire;

public static class WebApplicationExtensions
{
    public static void ConfigureRecurringJobs(this WebApplication app)
    {
        var recurringJobConfigurator = app.Services.GetRequiredService<IRecurringJobConfigurator>();
        recurringJobConfigurator.ConfigureRecurringJobs();
    }

    public static void UseHangfireDashboardWithBasicAuth(this WebApplication app)
    {
        app.UseHangfireDashboard("/hangfire");
    }
}
