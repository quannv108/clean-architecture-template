using Application.Abstractions.Locking;
using Medallion.Threading.Redis;
using StackExchange.Redis;

namespace Infrastructure.Locking;

/// <summary>
/// Redis-based distributed lock provider for high-performance multi-instance deployments.
/// </summary>
internal sealed class RedisDistributedLockProvider(IDatabase database) : IDistributedLockProvider
{
    private readonly RedisDistributedSynchronizationProvider _provider = new(database);

    public IDistributedLock CreateLock(string name)
    {
        var medallionLock = _provider.CreateLock(name);
        return new DistributedLockAdapter(medallionLock);
    }
}
