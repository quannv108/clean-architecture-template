using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
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
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[]
            {
                new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                {
                    RequireSsl = false,
                    SslRedirect = false,
                    LoginCaseSensitive = true,
                    Users = new[]
                    {
                        new BasicAuthAuthorizationUser
                        {
                            Login = "admin",
                            PasswordClear = "password"
                        }
                    }
                })
            }
        });
    }
}
