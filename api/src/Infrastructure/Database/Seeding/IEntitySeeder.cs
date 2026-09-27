using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Database.Seeding;

#pragma warning disable S2326
public interface IEntitySeeder<TEntity> where TEntity : Entity
#pragma warning restore S2326
{
    /// <summary>
    /// Seed system data (to system Tenant, or default data)
    /// </summary>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    Task SeedAsync(DbContext dbContext);

    /// <summary>
    /// Seed tenant specific data
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    Task SeedAsync(DbContext dbContext, Guid tenantId);
}
