using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Infrastructure.Database.Seeding;

internal class DbSeeder
{
    private readonly IServiceProvider _serviceProvider;

    public DbSeeder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task SeedAsync(DbContext dbContext)
    {
        return Task.CompletedTask;
    }

#pragma warning disable S1144
    private IEntitySeeder<T> Seeder<T>() where T : Entity
#pragma warning restore S1144
    {
        return _serviceProvider.GetRequiredService<IEntitySeeder<T>>();
    }
}
