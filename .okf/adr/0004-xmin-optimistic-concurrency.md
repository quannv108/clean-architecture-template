---
type: ADR
title: "ADR 0004: Optimistic concurrency with PostgreSQL xmin"
description: "Use the built-in xmin system column as the EF row version instead of a maintained version column or pessimistic locks."
tags: [adr, concurrency, postgresql, ef-core]
status: stable
generated:
  by: anthropic/claude-opus-5
  at: 2026-09-19T00:00:00Z
---

# ADR 0004: Optimistic concurrency with PostgreSQL xmin

**Status:** Accepted - reconstructed on 2026-09-19 from the codebase and its previous `docs/`
tree. The decision was already in force; this record was written afterwards, so the context and
alternatives are inferred. Correct them if you were there.

## Context

Two requests loading the same row and saving it will silently lose one of the writes unless something
detects the conflict. The options are a maintained version column (an extra column, and every write path has
to increment it), pessimistic locking (`SELECT FOR UPDATE`, which holds locks across think-time and invites
deadlocks), or PostgreSQL's own `xmin` system column.

## Decision

Map `xmin` to `Entity.Version` as the EF Core row version token. EF then appends `WHERE xmin = @version` to
every UPDATE; a mismatch affects zero rows and raises `DbUpdateConcurrencyException`.

[`ConcurrencyExceptionDecorator`](../../src/Application/Abstractions/Behaviors/ConcurrencyExceptionDecorator.cs) catches it and returns
[`ConcurrencyErrors.UpdateConflict()`](../../src/SharedKernel/Concurrency/ConcurrencyErrors.cs), which becomes **HTTP 412** for
the client to retry.

## Consequences

**Good.** No extra column and nothing to maintain - PostgreSQL updates `xmin` itself, so no write path can
forget. Zero cost until a conflict actually occurs. No locks held between load and save. Handlers need no
concurrency code at all.

**Costly.** It ties the solution to PostgreSQL; another provider needs a different row version strategy.
`xmin` is 32 bits and wraps, which is a theoretical concern on extremely high-churn tables. Clients must
handle 412 by reloading and retrying, which has to be documented in the API contract.

**Rule:** do not reach for a pessimistic lock before trying optimistic concurrency, and do not catch
`DbUpdateConcurrencyException` in a handler. Distributed locks are for *cross-instance coordination*, not
for entity updates - see [ADR 0007: PostgreSQL advisory locks by default, Redis when configured](0007-lock-provider-selection.md).

Mechanism: [Optimistic Concurrency](../engineering/patterns/optimistic-concurrency.md).
