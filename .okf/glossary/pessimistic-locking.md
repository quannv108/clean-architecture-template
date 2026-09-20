---
type: Term
title: "Pessimistic Locking"
description: "Taking a lock before reading so nobody else can change the row until you commit."
tags: [data, concurrency, database]
status: stable
---

# Pessimistic Locking

`SELECT ... FOR UPDATE` and similar. Correct but expensive: the lock is held across application think-time,
throughput drops, and inconsistent lock ordering produces deadlocks.

**Do not reach for it before trying [optimistic concurrency](optimistic-concurrency.md)**, which is already
wired and costs nothing until a conflict occurs. A [distributed lock](../engineering/patterns/distributed-lock.md) is a
different tool again - it coordinates work across instances, not writes to a row.
