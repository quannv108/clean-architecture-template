---
type: Pattern
title: "Optimistic Concurrency"
description: "Detect concurrent entity writes with the PostgreSQL xmin row version, and return 412 for the client to retry."
resource: src/SharedKernel/Concurrency
tags: [concurrency, ef-core, postgresql]
status: stable
---

# Optimistic Concurrency

## Problem

Two requests load the same row, both mutate it, both save. Without detection the second silently overwrites
the first.

## Solution

Nothing to write - it is already wired.

PostgreSQL's `xmin` system column is mapped to `Entity.Version` as an EF row version token, so EF appends
`WHERE xmin = @version` to every UPDATE. If the row changed since it was loaded, zero rows match and EF
throws `DbUpdateConcurrencyException`.
[`ConcurrencyExceptionDecorator`](../../../src/Application/Abstractions/Behaviors/ConcurrencyExceptionDecorator.cs) converts that to
[`ConcurrencyErrors.UpdateConflict()`](../../../src/SharedKernel/Concurrency/ConcurrencyErrors.cs) -> **HTTP 412**.

```csharp
var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
order.Confirm();
await db.SaveChangesAsync(ct);
// conflict -> Result.Failure(ConcurrencyErrors.UpdateConflict()) -> 412
```

## Rules

* **Do not catch `DbUpdateConcurrencyException` in a handler.** It is already handled; catching it locally
  usually swallows a retryable signal.
* **Do not reach for a pessimistic lock first.** `SELECT FOR UPDATE` holds a lock across think-time and
  invites deadlocks. Optimistic concurrency costs nothing until a conflict actually happens.
* **Distributed locks are a different tool** - they coordinate *work across instances*, not writes to a
  row. See [Distributed Lock](distributed-lock.md).
* **Document the 412 in your API contract**, because handling it is the client's job: reload, re-apply,
  retry.

## Choosing a concurrency mechanism

```
Concurrent writes to one entity?           -> optimistic concurrency (automatic)
Multiple SaveChanges that must be atomic?  -> explicit transaction  (atomic-transaction.md)
Coordinating work across instances?        -> distributed lock      (distributed-lock.md)
Bulk update with no domain logic?          -> ExecuteUpdate, last resort only (data-access.md)
```

Decision: [ADR 0004: Optimistic concurrency with PostgreSQL xmin](../../adr/0004-xmin-optimistic-concurrency.md).
