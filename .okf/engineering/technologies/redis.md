---
type: Technology
title: "Redis"
description: "Optional: enables the L2 cache tier and the Redis distributed lock provider when a connection string is configured."
resource: https://redis.io
tags: [technology, redis, caching, locking]
status: stable
---

# Redis

Entirely optional. One setting turns it on:

```json
{ "Redis": { "ConnectionString": "", "InstanceName": "CleanArchitecture:" } }
```

| Value | Effect |
|---|---|
| Empty | L1-only cache, PostgreSQL advisory locks |
| Populated | L1+L2 cache, Redis locks, Redis in `/health` |

One setting for both capabilities, so a deployment adds Redis once and gets shared caching and
higher-throughput locking together.

Add Redis when you run multiple instances and either cache staleness or lock contention becomes measurable.
Before that it is infrastructure to operate for no gain.

See [Caching Tiers](../../architecture/cross-cutting/caching-tiers.md) and
[ADR 0007: PostgreSQL advisory locks by default, Redis when configured](../../adr/0007-lock-provider-selection.md).
