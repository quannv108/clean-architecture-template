using Application.Abstractions.Locking;
using Medallion.Threading.Postgres;

namespace Infrastructure.Locking;

/// <summary>
/// PostgreSQL-based distributed lock provider using advisory locks.
/// </summary>
internal sealed class PostgresDistributedLockProvider(string connectionString) : IDistributedLockProvider
{
    private readonly PostgresDistributedSynchronizationProvider _provider = new(connectionString);

    public IDistributedLock CreateLock(string name)
    {
        // PostgreSQL provider requires PostgresAdvisoryLockKey
        // Enable hashing to support arbitrary lock names (e.g., "order:process:123")
        var medallionLock = _provider.CreateLock(new PostgresAdvisoryLockKey(name, allowHashing: true));
        return new DistributedLockAdapter(medallionLock);
    }
}
