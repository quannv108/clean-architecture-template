using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SharedKernel;

namespace Infrastructure.Database.Interceptors;

internal sealed class EntityIdGenerationInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        GenerateIdsForNewEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        GenerateIdsForNewEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void GenerateIdsForNewEntities(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var newEntities = context.ChangeTracker
            .Entries<Entity>()
            .Where(entry => entry.State == EntityState.Added && entry.Entity.Id == Guid.Empty)
            .Select(entry => entry.Entity);

        foreach (var entity in newEntities)
        {
            entity.Id = EntityIdGenerator.NewId();
        }
    }
}
