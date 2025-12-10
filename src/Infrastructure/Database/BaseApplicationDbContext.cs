using Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System.Linq.Expressions;

namespace Infrastructure.Database;

/// <summary>
/// Base DbContext that encapsulates shared model configuration and global filters
/// for both read-write and read-only contexts. Also provides default IReadOnlyApplicationDbContext
/// IQueryable<T> implementations that use AsNoTracking().
/// </summary>
internal abstract class BaseApplicationDbContext : DbContext
{
    protected BaseApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from the Infrastructure assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Set default schema
        modelBuilder.HasDefaultSchema(SchemaNameConstants.Default);

        // Apply global query filters (e.g., soft delete)
        ApplyGlobalQueryFilters(modelBuilder);

        // Configure optimistic concurrency for all entities using PostgreSQL xmin
        ConfigureOptimisticConcurrency(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>()
            .HaveConversion<string>();

        // Register global value converter for EncryptedString
        configurationBuilder
            .Properties<EncryptedString>()
            .HaveConversion<Database.Converters.EncryptedStringConverter>();

        base.ConfigureConventions(configurationBuilder);
    }

    private static void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(Entity.IsDeleted));
                var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(false)), parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    private static void ConfigureOptimisticConcurrency(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
            {
                // Configure Version property as concurrency token using PostgreSQL xmin
                modelBuilder.Entity(entityType.ClrType)
                    .Property<uint>(nameof(Entity.Version))
                    .IsRowVersion();
            }
        }
    }
}
