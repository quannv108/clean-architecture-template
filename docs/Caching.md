# Caching Strategy

## Overview

The application uses **HybridCache** with automatic two-tier architecture:
- **L1 (Level 1)**: In-memory cache (per instance)
- **L2 (Level 2)**: Redis distributed cache (shared across instances, optional)

L2 cache is automatically enabled when Redis is configured, providing seamless scaling from single to multi-instance deployments.

## Configuration

**Location**: [src/Infrastructure/DependencyInjection.cs](src/Infrastructure/DependencyInjection.cs) (AddCache method)

**appsettings.json**:
```json
{
  "Redis": {
    "ConnectionString": "",  // Empty = L1 only, Populated = L1 + L2
    "InstanceName": "CleanArchitecture:"
  }
}
```

**Development** (L1 only): Leave `ConnectionString` empty
- ✅ No infrastructure needed
- ⚠️ Cache not shared across instances

**Production** (L1 + L2): Set Redis connection string
- ✅ Cache synchronized across instances
- ✅ Health check at `/health` endpoint

## Architecture

**Cache Flow**:
1. Check L1 (in-memory) → Hit: return (<1ms)
2. Check L2 (Redis) if configured → Hit: return + populate L1 (1-5ms)
3. Query database → Populate L2 + L1 (10-50ms)

**Auto-Discovery**: HybridCache automatically detects `IDistributedCache` registration. If Redis is configured via `AddStackExchangeRedisCache()`, HybridCache uses it as L2.

**Limits**:
- Max payload: 1 MB
- Max key length: 1024 characters

## Usage Pattern

### CachedRepository Classes

**Location**: `src/Application/{Feature}/Data/`

**Interface**:
- `GetByIdAsync()` - Fetch with cache
- `RemoveCacheAsync()` - Invalidate cache

**Key method**: `cache.GetOrCreateAsync(key, factory, cancellationToken)`

### Cache Invalidation

**Patterns**:
| Operation | Action |
|-----------|--------|
| Create | None (not cached yet) |
| Read | Populate on miss |
| Update | Remove from cache |
| Delete | Remove from cache |

**Command handler pattern**:
1. Update entity
2. Save changes
3. Call `RemoveCacheAsync()`

**Multi-instance behavior**:
- With Redis: All instances synchronized via L2
- Without Redis: Stale data until L1 expires (max 5 min)

## Cache Key Convention

**Format**: `{entity}:{identifier}`

**Examples**:
- `users:{userId}`
- `roles:{roleId}`
- `profiles:{userId}`

**Rules**:
- ✅ Colon-separated, lowercase, descriptive
- ❌ No spaces, special characters (except colon), sensitive data

## When to Cache

**✅ Use CachedRepository**:
- Frequently accessed data (profiles, roles)
- Infrequently changing data
- Response time critical

**✅ Query database directly**:
- Frequently changing data
- Rarely repeated queries
- Complex filters
- Real-time accuracy required

**❌ Don't cache**:
- Complex filtered queries (cache key explosion)
- Real-time analytics
- Frequently updated counters

## Monitoring

**Metrics**:
- Cache hit rate: >80% target
- Response times: L1 <1ms, L2 1-5ms, DB 10-50ms
- Memory usage for L1 cache

**Logging**: Startup logs show configuration mode (L1 only vs L1+L2)

**Health check**: `/health` endpoint includes Redis status when configured

## Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| **Every request hits DB** | HybridCache not injected, inconsistent keys | Verify DI, check key generation |
| **Stale data** | Cache not invalidated | Call `RemoveCacheAsync()` after updates |
| **High memory** | Too much cached, no expiration | Review cached data, set limits, use pagination |
| **Redis errors** | Connection issues | Check connection string, Redis health, network |

**Fallback**: HybridCache gracefully degrades to L1-only if Redis fails

## Best Practices

### 1. Cache Selection
- ✅ User profiles, roles, reference data
- ❌ Real-time analytics, session state

### 2. Key Naming
```csharp
// ✅ Good
$"users:{userId}"

// ❌ Bad
$"user_{userId}_data_v2_new"
```

### 3. Always Invalidate on Updates
```csharp
await context.SaveChangesAsync(ct);
await cache.RemoveCacheAsync(entityId, ct); // Don't forget!
```

### 4. Return DTOs, Never Domain Entities
```csharp
// ✅ Good: Return Response DTOs
Select(u => new UserResponse(...))

// ❌ Bad: Cache domain entities
```

### 5. Appropriate Expiration Times
- Frequently changing: 2-5 minutes
- Stable data: 15-30 minutes
- Balance consistency vs performance

## Dependencies

- **Microsoft.Extensions.Caching.Hybrid**
- **Microsoft.Extensions.Caching.StackExchangeRedis**
- **StackExchange.Redis**

## References

- [HybridCache Documentation](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid)
- [Distributed Caching in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed)

---