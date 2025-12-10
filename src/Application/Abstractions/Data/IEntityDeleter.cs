using SharedKernel;

namespace Application.Abstractions.Data;

public interface IEntityDeleter
{
    Task HardDeleteAsync<T>(T entity) where T : Entity;
    Task SoftDeleteAsync<T>(T entity) where T : Entity;

    /// <summary>
    /// Soft deletes all entities of type T that have the specified UserId.
    /// Useful for cascade deletion when a user is deleted.
    /// </summary>
    /// <typeparam name="T">The entity type to delete</typeparam>
    /// <param name="userId">The user ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SoftDeleteEntitiesByUserIdAsync<T>(Guid userId, CancellationToken cancellationToken) where T : Entity;
}
