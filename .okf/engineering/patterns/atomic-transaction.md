---
type: Pattern
title: "Atomic Transaction"
description: "One load, N in-memory mutations, one save - and when an explicit transaction is actually needed."
tags: [transactions, ef-core, unit-of-work]
status: stable
---

# Atomic Transaction

## The default: EF Unit of Work

`DbContext` **is** a unit of work. One `SaveChangesAsync` wraps every pending change - including the
[`OutboxMessage`](../../domains/outbox/outbox-message.md) rows created by domain events - in a single database
transaction. No explicit transaction is required.

**1 load (all the data up front) -> N in-memory mutations -> 1 save.**

Load with `Include()` for everything the operation touches, call the behaviour methods, then one
`SaveChangesAsync` - see [Command Handler](command-handler.md).

## When an explicit transaction is warranted

Only when several `SaveChangesAsync` calls must succeed together, or when mixing EF with raw SQL:

```csharp
await using var tx = await db.Database.BeginTransactionAsync(ct);
// ... several SaveChangesAsync calls, or raw SQL ...
await tx.CommitAsync(ct);
```

If you find yourself needing this often, the aggregate boundaries are probably in the wrong place.

## What to avoid

* Splitting one logical load across several queries when a single `Include()` covers it.
* Calling `SaveChangesAsync` more than once in a handler "to be safe" - it makes the operation
  non-atomic, which is the opposite.
* `ExecuteUpdate` / `ExecuteDelete` in a handler - every side effect silently skips;
  [Data Access](../../architecture/cross-cutting/data-access.md) lists them.
