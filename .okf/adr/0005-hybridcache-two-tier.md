---
type: ADR
title: "ADR 0005: Two-tier caching with HybridCache"
description: "Use HybridCache so the same code path serves an L1-only development setup and an L1+L2 Redis deployment."
tags: [adr, caching, hybridcache, redis, performance]
status: stable
generated:
  by: anthropic/claude-opus-5
  at: 2026-09-19T00:00:00Z
---

# ADR 0005: Two-tier caching with HybridCache

**Status:** Accepted - reconstructed on 2026-09-19 from the codebase and its previous `docs/`
tree. The decision was already in force; this record was written afterwards, so the context and
alternatives are inferred. Correct them if you were there.

## Context

A cache that requires Redis makes local development need infrastructure. A cache that is memory-only cannot
be shared between instances, so a write on one instance leaves the others stale. Writing both and switching
between them means two code paths, and the one not used locally is the one that breaks.

## Decision

Use `Microsoft.Extensions.Caching.Hybrid`. One API, two tiers: in-memory L1 always, Redis L2 when
`Redis:ConnectionString` is populated - HybridCache discovers the registered `IDistributedCache` and uses
it automatically.

Reads go through [cached repositories](../../src/Application) in
`Application/<Feature>/Data/`, returning DTOs.

## Consequences

**Good.** Development needs nothing installed; production scales by setting a connection string. One code
path, so the deployed behaviour is the one that was developed against. HybridCache also handles stampede
protection, and degrades to L1 if Redis fails rather than failing the request.

**Costly.** Without Redis, instances can serve stale data until L1 expires - acceptable for the reference
data this is used on, not for anything a user must see immediately after their own write. Payloads are
capped at 1 MB and keys at 1024 characters. The behavioural difference between the two modes is real even
though the code is identical, so cache-sensitive behaviour should be tested with L2 enabled.

**Rules:** return DTOs, never entities; invalidate after every write that changes cached data; do not cache
complex filtered queries, where the key space explodes. See
[Cached Read](../engineering/patterns/cached-read.md).
