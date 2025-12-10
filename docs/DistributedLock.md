# Distributed Locking

## Overview

Distributed locks coordinate work across multiple application instances, preventing race conditions and duplicate processing. The system automatically selects between **PostgreSQL advisory locks** (default) and **Redis locks** (high-performance) based on configuration.

**Key Abstraction**: `IDistributedLockProvider` in Application layer with zero infrastructure dependencies. Implementations in Infrastructure layer use the [Medallion.Threading](https://github.com/madelson/DistributedLock) library via adapter pattern.

## Configuration

Provider is automatically selected based on `Redis:ConnectionString` in `appsettings.json`:

| Configuration | Provider | Use Case |
|--------------|----------|----------|
| `ConnectionString: ""` (empty) | **PostgreSQL** | Default, development, single/few instances |
| `ConnectionString: "host:6379,..."` | **Redis** | Multi-instance deployments, high traffic |

When Redis is configured, you get **both** distributed locking and distributed caching. See [Caching.md](Caching.md) for details.

## Usage

**Basic Pattern:**

```csharp
public class ProcessOrderCommandHandler(
    IDistributedLockProvider lockProvider) : ICommandHandler<ProcessOrderCommand>
{
    public async Task<Result> Handle(ProcessOrderCommand command, CancellationToken ct)
    {
        var lockName = $"order:process:{command.OrderId}";

        await using var lockHandle = await lockProvider
            .CreateLock(lockName)
            .TryAcquireAsync(TimeSpan.FromSeconds(30), ct);

        if (lockHandle is null)
            return Result.Failure(OrderErrors.AlreadyBeingProcessed(command.OrderId));

        // Critical section - only one instance executes this
        // Lock is automatically released when lockHandle is disposed
    }
}
```

**Timeout Guidelines:**
- Background jobs: `TimeSpan.Zero` (try immediately, skip if locked)
- User requests: `TimeSpan.FromSeconds(5-10)` (wait reasonably)
- Long operations: `TimeSpan.FromMinutes(5-10)` (generous timeout)

## Lock Naming Convention

**Format:** `{entity}:{operation}:{id}`

**Examples:**
- Per-resource: `order:process:123`, `payment:refund:456`, `user:login:email@example.com`
- Global jobs: `job:process-outbox`, `job:cleanup-tokens`

**Best Practices:** Use lowercase, colon-separated, descriptive names under 100 characters. Avoid sensitive data (lock names may be logged).

## Provider Comparison

| Feature | PostgreSQL (Default) | Redis |
|---------|---------------------|-------|
| **Latency** | ~1-5ms | ~1-10ms |
| **Throughput** | High (adequate for most) | Very high (100K+ ops/sec) |
| **Infrastructure** | Existing database | Requires Redis |
| **Setup** | Automatic | Add connection string |
| **Best For** | Single/few instances, cost-conscious | Multi-instance, high traffic |
| **Bonus** | - | Enables distributed caching ([Caching.md](Caching.md)) |

**Performance Note:** Difference is typically 2-10ms - negligible for most applications. Start with PostgreSQL (default), add Redis when needed.

## When to Use Distributed Locks

### ✅ Use Cases

1. **Duplicate Processing Prevention** - Background jobs across multiple instances
2. **Rate Limiting per Resource** - One login attempt per user at a time
3. **Idempotent External API Calls** - Payment processing, third-party integrations
4. **Leader Election** - Only one instance performs a scheduled task
5. **Bulk Operations Coordination** - Multiple workers processing large datasets

### ❌ When NOT to Use

1. **Regular Entity Updates** → Use **optimistic concurrency** (automatic via `xmin` - see [Architecture.md](Architecture.md))
2. **ACID Transactions** → Use database transactions
3. **High-Frequency Operations** → Use caching, queues, CQRS patterns
4. **Read-Only Queries** → No locking needed

## Distributed Locks vs Optimistic Concurrency

| Feature | Distributed Locks | Optimistic Concurrency |
|---------|------------------|------------------------|
| **Purpose** | Prevent concurrent execution | Detect concurrent modifications |
| **Scope** | Application-level (cross-instance) | Database-level (per entity) |
| **When** | Before operation starts | When saving changes |
| **Performance** | ~1-10ms overhead | No overhead until conflict |
| **Use Case** | External APIs, background jobs | Entity updates, business logic |
| **Implementation** | Manual via `IDistributedLockProvider` | Automatic via `xmin` column |
| **On Conflict** | Returns null handle | Throws `DbUpdateConcurrencyException` |

**Rule of Thumb:** Use optimistic concurrency for entity updates (automatic). Use distributed locks for cross-instance coordination and external API calls (manual).

## Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| **Lock not acquired** (null) | Another instance processing | Background jobs: skip. User requests: return error |
| **Timeout** | Operation too slow | Increase timeout, optimize code, break into chunks |
| **Deadlock** | Inconsistent lock ordering | Acquire locks in same order, avoid nesting |
| **High contention** | Too many instances competing | Use granular locks (per-item vs global) |
| **Lock held too long** | Critical section slow | Move non-critical work outside lock |

**Automatic Lock Release:** Locks are released when process crashes (PostgreSQL closes connection, Redis TTL expires).

## Monitoring

**Startup Logging:** Provider selection is logged automatically:
- `Distributed locks: PostgreSQL advisory locks (using existing database)`
- `Distributed locks: Redis provider (high-performance mode)`

**Health Checks:** Available at `/health` endpoint. PostgreSQL always monitored; Redis added when configured.

**Key Metrics to Track:**
- Lock acquisition rate & contention rate
- Lock hold duration
- Skip rate (background jobs)

## Best Practices

1. **Minimize Lock Scope** - Acquire locks only for critical sections, not entire operations
2. **Use Appropriate Timeouts** - Zero for background jobs, 5-10s for user requests
3. **Granular Locking** - Lock per-resource (`order:process:123`) instead of global (`process-all-orders`)
4. **Non-Blocking for Background Jobs** - Use `TimeSpan.Zero` to skip if locked
5. **Consistent Ordering** - Always acquire multiple locks in same order to prevent deadlocks


## FAQ

**Q: Do I need Redis?**
A: No. PostgreSQL works great for most scenarios. Add Redis for high traffic or when you need distributed caching.

**Q: What happens if a process crashes while holding a lock?**
A: Lock is automatically released (PostgreSQL closes connection, Redis TTL expires).

**Q: Should I use locks for entity updates?**
A: No. Use optimistic concurrency (automatic via `xmin`). Use locks for cross-instance coordination and external APIs.

**Q: Can I nest locks?**
A: Avoid it. If necessary, always acquire in the same order to prevent deadlocks.

---

**Related Documentation:**
- [Caching.md](Caching.md) - HybridCache and Redis configuration
- [Architecture.md](Architecture.md) - Optimistic concurrency with `xmin`
- [OutboxPattern.md](OutboxPattern.md) - Real-world lock usage example
- [Medallion.Threading](https://github.com/madelson/DistributedLock) - Underlying library
