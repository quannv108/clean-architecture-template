---
type: Mechanism
title: Caching Tiers
description: The HybridCache L1/L2 topology - in-memory per instance, optional Redis shared across instances, selected by configuration.
tags: [caching, hybridcache, redis, performance]
status: stable
---

# Caching Tiers

[`HybridCache`](../../engineering/technologies/hybrid-cache.md) gives two tiers behind one API.

| Tier | Store | Scope | Typical latency |
|---|---|---|---|
| **L1** | In-memory | One process | < 1 ms |
| **L2** | [Redis](../../engineering/technologies/redis.md) | All instances | 1-5 ms |
| (miss) | PostgreSQL | - | 10-50 ms |

Lookup order is L1, then L2, then the database - populating each tier on the way back.

## Selection is configuration, not code

```json
{
  "Redis": {
    "ConnectionString": "",
    "InstanceName": "CleanArchitecture:"
  }
}
```

* **Empty** - L1 only. Nothing to install; correct for development and single-instance deployments. Entries
  are not shared, so another instance can serve stale data until L1 expires.
* **Populated** - `AddStackExchangeRedisCache` registers an `IDistributedCache`, HybridCache discovers it
  and uses it as L2 automatically. The same setting also switches the
  [distributed lock provider](../../engineering/patterns/distributed-lock.md) to Redis and adds Redis to `/health`.

Wired in `src/Infrastructure/DependencyInjection.cs` (`AddCache`). If Redis fails at runtime, HybridCache
degrades to L1 rather than failing the request.

## Limits

Maximum payload 1 MB; maximum key length 1024 characters.

## Targets worth watching

Hit rate above 80%; L1 under 1 ms; L2 1-5 ms. Startup logs state which mode is active.

## Related

* [Cached Read](../../engineering/patterns/cached-read.md) - how to write and invalidate a cached read
* [Cached Read](../../engineering/patterns/cached-read.md) - the `{entity}:{identifier}` format
* [ADR 0005: Two-tier caching with HybridCache](../../adr/0005-hybridcache-two-tier.md)
