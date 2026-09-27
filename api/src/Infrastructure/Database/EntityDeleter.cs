using System.Linq.Expressions;
using Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Database;

internal sealed class EntityDeleter(IApplicationDbContext context) : IEntityDeleter
{
    public async Task HardDeleteAsync<T>(T entity) where T : Entity
    {
        ArgumentNullException.ThrowIfNull(entity);

        context.Set<T>().Remove(entity);
        entity.RaiseDeleteEvent();
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Soft delete an entity
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="T"></typeparam>
    public async Task SoftDeleteAsync<T>(T entity) where T : Entity
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.IsDeleted = true;
        entity.RaiseDeleteEvent();
        context.Set<T>().Update(entity);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Soft deletes all entities of type T that have the specified UserId.
    /// </summary>
    public async Task SoftDeleteEntitiesByUserIdAsync<T>(Guid userId, CancellationToken cancellationToken)
        where T : Entity
    {
        // Check if the entity type has a UserId property
        var userIdProperty = typeof(T).GetProperty("UserId", typeof(Guid));
        if (userIdProperty is null)
        {
            return; // Entity type doesn't have UserId, nothing to delete
        }

        // Build expression for UserId filter via reflection
        var parameter = Expression.Parameter(typeof(T));
        var userIdMember = Expression.Property(parameter, userIdProperty);
        var userIdConstant = Expression.Constant(userId);
        var userIdEqual = Expression.Equal(userIdMember, userIdConstant);
        var userIdLambda = Expression.Lambda<Func<T, bool>>(userIdEqual, parameter);

        await SoftDeleteEntitiesAsync(userIdLambda, cancellationToken);
    }

    private async Task SoftDeleteEntitiesAsync<T>(Expression<Func<T, bool>> condition,
        CancellationToken cancellationToken) where T : Entity
    {
        // Get all entities matching the criteria using normal LINQ for IsDeleted check
        var entities = await context.Set<T>()
            .Where(condition)
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);

        // Soft delete each entity
        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
            entity.RaiseDeleteEvent();
            context.Set<T>().Update(entity);
        }

        // Save all changes in one batch
        await context.SaveChangesAsync(cancellationToken);
    }
}
