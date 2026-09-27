namespace Application.Abstractions.Locking;

/// <summary>
/// Provides distributed locks for coordinating work across multiple application instances.
/// </summary>
/// <remarks>
/// Implementation is automatically selected based on infrastructure:
/// - Redis (if configured): High-performance locks for multi-instance deployments
/// - PostgreSQL (default): Advisory locks using existing database infrastructure
/// </remarks>
public interface IDistributedLockProvider
{
    /// <summary>
    /// Creates a distributed lock with the specified name.
    /// </summary>
    /// <param name="name">
    /// The unique name of the lock. Use format: {entity}:{operation}:{id}
    /// Examples: "order:process:123", "payment:refund:456"
    /// </param>
    /// <returns>A distributed lock instance that can be acquired and released</returns>
    IDistributedLock CreateLock(string name);
}

/// <summary>
/// Represents a distributed lock that can be acquired and released.
/// </summary>
public interface IDistributedLock
{
    /// <summary>
    /// Attempts to acquire the lock within the specified timeout period.
    /// </summary>
    /// <param name="timeout">The maximum time to wait for the lock</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A handle to the acquired lock, or null if the lock could not be acquired within the timeout.
    /// The handle should be disposed to release the lock.
    /// </returns>
    Task<IDistributedLockHandle?> TryAcquireAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a handle to an acquired distributed lock.
/// Dispose this handle to release the lock.
/// </summary>
public interface IDistributedLockHandle : IAsyncDisposable
{
}
