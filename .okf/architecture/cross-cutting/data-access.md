---
type: Mechanism
title: Data Access
description: Reads go through cached repositories returning DTOs; writes inject the DbContext directly. The two paths never cross.
tags: [data-access, ef-core, cqrs, caching]
status: stable
---

# Data Access

Two paths, chosen by direction of travel.

## Read path

* Lives in `src/Application/<Feature>/Data/` as a [cached repository](../../../src/Application).
* Uses [`HybridCache`](../../engineering/technologies/hybrid-cache.md) via `GetOrCreateAsync`.
* **Returns response DTOs, never domain entities.** A cached entity is a detached, stale, mutable copy of
  state the domain believes it owns.
* Queries are `AsNoTracking` projections.
* Reads that are filtered, ad hoc or need real-time accuracy skip the cache and query
  [`IReadOnlyApplicationDbContext`](../../../src/Application/Abstractions/Data/IReadOnlyApplicationDbContext.cs) directly.

See [Caching Tiers](caching-tiers.md) and [Cached Read](../../engineering/patterns/cached-read.md).

## Write path

* Command handlers inject [`IApplicationDbContext`](../../../src/Application/Abstractions/Data/IApplicationDbContext.cs).
* **Never use a cached repository in a command handler** - you would be mutating a DTO or reading stale
  state before a write.
* The shape is **1 load -> N in-memory mutations -> 1 save**:

```csharp
var order = await dbContext.Orders
    .Include(o => o.Items)
    .FirstOrDefaultAsync(o => o.Id == id, ct);

order.ApplyDiscount(code);
order.Recalculate();

await dbContext.SaveChangesAsync(ct);   // atomic: entity + outbox messages
```

* `DbContext` is a Unit of Work. One `SaveChangesAsync` wraps the entity changes **and** the
  [`OutboxMessage`](../../domains/outbox/outbox-message.md) rows in one transaction. See
  [Atomic Transaction](../../engineering/patterns/atomic-transaction.md).
* After a write that changes cached data, call the repository's `RemoveCacheAsync(...)`.

## Built-in behaviours you get for free

| Behaviour | Mechanism |
|---|---|
| Soft delete | Global query filter `IsDeleted == false` on every `Entity` - [Soft Delete](../../engineering/patterns/soft-delete.md) |
| Optimistic concurrency | PostgreSQL `xmin` mapped to `Entity.Version` - [Optimistic Concurrency](../../engineering/patterns/optimistic-concurrency.md) |
| Id assignment | [`EntityIdGenerationInterceptor`](../../../src/Infrastructure/Database/Interceptors/EntityIdGenerationInterceptor.cs) |
| Audit stamps | [`AuditableEntityInterceptor`](../../../src/Infrastructure/Database/Interceptors/AuditableEntityInterceptor.cs) |
| Enum storage | Stored as strings by convention in [`BaseApplicationDbContext`](../../../src/Infrastructure/Database/BaseApplicationDbContext.cs) |
| Field encryption | [`EncryptedStringConverter`](../../../src/Infrastructure/Database/Converters/EncryptedStringConverter.cs), registered globally |

## What not to do

`ExecuteUpdateAsync` / `ExecuteDeleteAsync` issue SQL directly, so change tracking never runs:

* **Domain events are not raised** - nothing downstream happens, and nothing reports that it did not.
* **No `OutboxMessage` rows** are written.
* **Interceptors do not run** - no audit stamps, no id generation.
* **Soft delete is bypassed** by `ExecuteDelete`, which removes rows for real.
* **Optimistic concurrency is bypassed** - the `xmin` check never happens.

Every one of these is silent: the command returns success and the side effects never occur, which surfaces
days later as "the email never arrived". They are permitted only for operational maintenance with no domain
meaning, outside a command handler, with a comment saying why the missing side effects are acceptable. No
test bans them; review does. Use load -> behaviour method -> `SaveChangesAsync` instead -
[Command Handler](../../engineering/patterns/command-handler.md).
