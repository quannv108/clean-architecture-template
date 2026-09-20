---
type: Term
title: "Advisory Lock"
description: "A PostgreSQL lock with application-defined meaning, not attached to any row or table."
tags: [data, locking, postgresql]
status: stable
---

# Advisory Lock

The database provides the mutual exclusion; the application decides what the lock means. Released
automatically when the holding connection closes, so a crashed process cannot hold one forever.

The default [distributed lock](../engineering/patterns/distributed-lock.md) provider here - no extra infrastructure,
because the database is already there. See
[`PostgresDistributedLockProvider`](../../src/Infrastructure/Locking/PostgresDistributedLockProvider.cs).
