---
type: ADR
title: "ADR 0007: PostgreSQL advisory locks by default, Redis when configured"
description: "Provide distributed locking through one abstraction with a provider chosen by the same setting that enables the L2 cache."
tags: [adr, locking, postgresql, redis, concurrency]
status: stable
generated:
  by: anthropic/claude-opus-5
  at: 2026-09-19T00:00:00Z
---

# ADR 0007: PostgreSQL advisory locks by default, Redis when configured

**Status:** Accepted - reconstructed on 2026-09-19 from the codebase and its previous `docs/`
tree. The decision was already in force; this record was written afterwards, so the context and
alternatives are inferred. Correct them if you were there.

## Context

Some work must not run twice at once across instances: a recurring job, a payment call, leader election.
That needs a lock outside the process. Redis is the usual answer, but requiring Redis to run a two-instance
deployment - or to run locally - is a heavy prerequisite for a capability most systems use rarely.

## Decision

[`IDistributedLockProvider`](../../src/Application/Abstractions/Locking/IDistributedLockProvider.cs) in Application, implemented over
[Medallion.Threading](../engineering/technologies/medallion-threading.md), with the provider chosen by configuration:

| `Redis:ConnectionString` | Provider |
|---|---|
| empty | [PostgreSQL advisory locks](../../src/Infrastructure/Locking/PostgresDistributedLockProvider.cs) |
| populated | [Redis locks](../../src/Infrastructure/Locking/RedisDistributedLockProvider.cs) |

The same setting enables the [L2 cache tier](../architecture/cross-cutting/caching-tiers.md), so a deployment adds Redis once
and gets both.

## Consequences

**Good.** Locking works out of the box with no extra infrastructure, since the database is already there.
PostgreSQL releases advisory locks when the connection closes, so a crashed process cannot hold one
forever. Moving to Redis is a configuration change, not a code change.

**Costly.** The two providers do not have identical semantics - Redis locks expire on a TTL, so a critical
section longer than the TTL can lose its lock while still running, whereas an advisory lock is held for the
connection's life. Code must be written for the weaker guarantee: small critical sections. Advisory locks
also consume a database connection for their duration.

**Rule:** locks are for cross-instance coordination, **not** for entity updates - those are handled
automatically by [optimistic concurrency](0004-xmin-optimistic-concurrency.md). Background jobs should
acquire with `TimeSpan.Zero` and skip rather than queue. Naming:
[Distributed Lock](../engineering/patterns/distributed-lock.md).
