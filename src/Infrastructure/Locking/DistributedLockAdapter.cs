using Application.Abstractions.Locking;

namespace Infrastructure.Locking;

/// <summary>
/// Adapter that wraps Medallion.Threading.IDistributedLock to implement our Application layer abstraction.
/// </summary>
internal sealed class DistributedLockAdapter(Medallion.Threading.IDistributedLock medallionLock)
    : IDistributedLock
{
    public async Task<IDistributedLockHandle?> TryAcquireAsync(TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var handle = await medallionLock.TryAcquireAsync(timeout, cancellationToken);
        return handle is not null ? new DistributedLockHandleAdapter(handle) : null;
    }
}

/// <summary>
/// Adapter that wraps the Medallion.Threading lock handle to implement our Application layer abstraction.
/// </summary>
internal sealed class DistributedLockHandleAdapter(IAsyncDisposable medallionHandle)
    : IDistributedLockHandle
{
    public ValueTask DisposeAsync()
    {
        return medallionHandle.DisposeAsync();
    }
}
