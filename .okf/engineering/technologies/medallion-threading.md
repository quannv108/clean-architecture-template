---
type: Technology
title: "Medallion.Threading"
description: "The distributed locking library behind both lock providers."
resource: https://github.com/madelson/DistributedLock
tags: [technology, locking, concurrency]
status: stable
---

# Medallion.Threading

Provides the PostgreSQL advisory lock and Redis lock implementations, wrapped by
[`DistributedLockAdapter`](../../../src/Infrastructure/Locking/DistributedLockAdapter.cs) so that Application code sees only
[`IDistributedLockProvider`](../../../src/Application/Abstractions/Locking/IDistributedLockProvider.cs).

The library type never escapes Infrastructure, so swapping it is a change to one adapter.

Semantics differ by provider and the difference matters: PostgreSQL advisory locks are held for the life of
the connection, Redis locks expire on a TTL. Write for the weaker guarantee - small critical sections. See
[Distributed Lock](../patterns/distributed-lock.md).
