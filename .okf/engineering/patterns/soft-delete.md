---
type: Pattern
title: "Soft Delete"
description: "Mark rows deleted instead of removing them, hidden by a model-wide query filter."
resource: src/SharedKernel/Entity.cs
tags: [soft-delete, ef-core, data-retention]
status: stable
---

# Soft Delete

Every [`Entity`](../../../src/SharedKernel/Entity.cs) carries `IsDeleted`, and
[`BaseApplicationDbContext`](../../../src/Infrastructure/Database/BaseApplicationDbContext.cs) applies a global query filter
(`IsDeleted == false`) to every entity type. Ordinary queries need no predicate - the filter is part of the
model, so it cannot be forgotten.

Delete through [`IEntityDeleter`](../../../src/Application/Abstractions/Data/IEntityDeleter.cs).

## Rules

* **Never set `IsDeleted` by hand.** One code path, one behaviour.
* **Never `ExecuteDelete`** - it removes rows for real and skips every side effect -
  [Data Access](../../architecture/cross-cutting/data-access.md).
* `IgnoreQueryFilters()` exists for administrative queries. It returns deleted rows silently, so use it
  deliberately and never in a normal read path.

## What to watch for

* **Unique constraints must account for deleted rows**, or re-creating something with the same natural key
  fails against a row nobody can see. Use a filtered unique index.
* **Tables grow forever.** Plan retention for high-volume ones -
  [`DeleteOldAuditLogsCommand`](../../../src/Application/AuditLogs/DeleteOldAuditLogsCommand.cs) is the shipped example.
* **Reporting outside EF Core does not get the filter.** Anything reading the database directly must apply
  `IsDeleted = false` itself.
* **A soft-deleted parent does not cascade.** Children keep their own flag; decide explicitly whether they
  are deleted with it.

Hard deletion required by regulation - a right-to-erasure request, for instance - is an explicit maintenance
operation, not a handler.

Decision: [ADR 0009: Soft delete with a global query filter](../../adr/0009-soft-delete-by-default.md).
