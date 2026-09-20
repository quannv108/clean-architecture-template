---
type: Term
title: "L2 Cache"
description: "The distributed cache tier, shared across instances, backed by Redis when configured."
tags: [caching, redis, performance]
status: stable
---

# L2 Cache

1-5 ms, and optional. Enabled by populating `Redis:ConnectionString` - HybridCache discovers the registered
`IDistributedCache` and uses it with no code change.

With L2, instances see each other's invalidations. Without it, they do not. The same setting also switches
the [distributed lock](../engineering/patterns/distributed-lock.md) provider to Redis.
