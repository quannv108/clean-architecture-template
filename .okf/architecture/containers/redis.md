---
type: Container
title: "Redis"
description: "Optional. One connection string turns on the shared L2 cache tier and the Redis distributed lock provider together."
tags: [container, c4, redis, caching, locking]
status: stable
---

# Redis

Entirely optional, and off by default.

```json
{ "Redis": { "ConnectionString": "", "InstanceName": "CleanArchitecture:" } }
```

| Setting | Effect |
|---|---|
| Empty | L1-only cache; PostgreSQL advisory locks; Redis absent from `/health` |
| Populated | L1+L2 cache; Redis locks; Redis checked by `/health` |

One switch for both capabilities, so a deployment adds Redis once and gets shared caching and
higher-throughput locking together.

## When to add it

When you run more than one [Web.Api](web-api.md) instance **and** either cache staleness between instances
or lock contention becomes measurable. Before that it is infrastructure to operate for no gain — the
PostgreSQL lock provider is within a few milliseconds of Redis.

## What changes when you do

* Cache invalidation becomes visible to all instances. Without Redis, an instance serves stale data until
  its own L1 entry expires.
* Lock semantics change: Redis locks expire on a **TTL**, so a critical section longer than the TTL can
  lose its lock while still running. Advisory locks are held for the connection's life. Write for the
  weaker guarantee — small critical sections.

See [Caching Tiers](../cross-cutting/caching-tiers.md) and
[ADR 0007: PostgreSQL advisory locks by default, Redis when configured](../../adr/0007-lock-provider-selection.md).
