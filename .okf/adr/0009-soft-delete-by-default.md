---
type: ADR
title: "ADR 0009: Soft delete with a global query filter"
description: "Mark rows deleted rather than removing them, and hide them with a model-wide query filter."
tags: [adr, soft-delete, ef-core, data-retention]
status: stable
generated:
  by: anthropic/claude-opus-5
  at: 2026-09-19T00:00:00Z
---

# ADR 0009: Soft delete with a global query filter

**Status:** Accepted - reconstructed on 2026-09-19 from the codebase and its previous `docs/`
tree. The decision was already in force; this record was written afterwards, so the context and
alternatives are inferred. Correct them if you were there.

## Context

Physical deletion loses history, breaks foreign keys from audit and log tables, and makes "who deleted this
and when" unanswerable. It is also irreversible at exactly the moment somebody wishes it were not.

The risk of soft delete is the opposite: a query that forgets the filter returns deleted rows, which is a
silent correctness bug rather than an error.

## Decision

[`Entity`](../../src/SharedKernel/Entity.cs) carries `IsDeleted`, and
[`BaseApplicationDbContext`](../../src/Infrastructure/Database/BaseApplicationDbContext.cs) applies a global query filter
(`IsDeleted == false`) to every entity type, so the filter cannot be forgotten. Deletion goes through
[`IEntityDeleter`](../../src/Application/Abstractions/Data/IEntityDeleter.cs).

## Consequences

**Good.** History is preserved and deletes are reversible. Foreign keys from audit records stay valid.
Ordinary queries need no predicate - the filter is applied by the model, not by discipline.

**Costly.** Tables grow forever unless a retention policy prunes them. Unique constraints must account for
deleted rows, or re-creating something with the same natural key fails. `IgnoreQueryFilters()` exists and
silently returns deleted rows, so it needs care. Reporting that joins outside EF Core does not get the
filter at all.

**Rules:** never set `IsDeleted` by hand; never `ExecuteDelete`
([Data Access](../architecture/cross-cutting/data-access.md)). Hard deletion
required by regulation is an explicit maintenance operation, not a handler.
