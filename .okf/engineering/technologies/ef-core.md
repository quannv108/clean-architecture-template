---
type: Technology
title: "Entity Framework Core"
description: "The ORM: contexts, configurations, interceptors, value converters, query filters and migrations."
resource: https://learn.microsoft.com/ef/core
tags: [technology, ef-core, orm, persistence]
status: stable
---

# Entity Framework Core

Everything persistence-related lives in `src/Infrastructure/Database` - see
[Persistence](../../architecture/cross-cutting/persistence.md).

Features this codebase depends on:

| Feature | Used for |
|---|---|
| Global query filters | [Soft delete](../patterns/soft-delete.md) |
| Row version tokens | [`xmin` optimistic concurrency](../patterns/optimistic-concurrency.md) |
| Value converters | [`EncryptedString`](../../../src/SharedKernel/EncryptedString.cs), enums as strings |
| SaveChanges interceptors | Id generation, audit stamps |
| `IEntityTypeConfiguration<T>` | Per-entity mapping, discovered by assembly scan |
| Unit of Work | One `SaveChangesAsync` per handler - [Atomic Transaction](../patterns/atomic-transaction.md) |

**Do not use `ExecuteUpdate` / `ExecuteDelete`** in command handlers - they bypass change tracking, domain
events and the Outbox, silently. See
[Data Access](../../architecture/cross-cutting/data-access.md).

Migrations have their own procedure with two mandatory flags and a visibility change:
[Add an EF Core Migration](../../workflows/engineering/add-ef-migration.md).
