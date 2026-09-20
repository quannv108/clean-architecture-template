---
type: Technology
title: "HybridCache"
description: "The two-tier caching API: in-memory L1 plus an optional distributed L2, behind one interface."
resource: https://learn.microsoft.com/aspnet/core/performance/caching/hybrid
tags: [technology, caching, performance]
status: stable
---

# HybridCache

`Microsoft.Extensions.Caching.Hybrid`. One API over both tiers, with stampede protection and automatic L2
discovery - if an `IDistributedCache` is registered, it is used as L2 with no code change.

Used by [cached repositories](../../../src/Application) via `GetOrCreateAsync`.

Limits: 1 MB payload, 1024-character keys. Degrades to L1 if Redis fails rather than failing the request.

See [Cached Read](../patterns/cached-read.md),
[Caching Tiers](../../architecture/cross-cutting/caching-tiers.md), and
[ADR 0005: Two-tier caching with HybridCache](../../adr/0005-hybridcache-two-tier.md).
