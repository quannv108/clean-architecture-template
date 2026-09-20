---
type: Container
title: "PostgreSQL Database"
description: "The application database, and the source of the xmin row version and advisory locks the architecture depends on."
resource: src/Infrastructure/Database
tags: [container, c4, postgresql, database]
status: stable
---

# PostgreSQL Database

The one required data store. Run locally as an Aspire container named `db`; in a deployed environment it is
a managed instance.

## What it holds

| Data | Concept |
|---|---|
| Business entities | [Domains](../../domains/index.md) |
| `OutboxMessage` rows | [OutboxMessage](../../domains/outbox/outbox-message.md) |
| `AuditLog` rows | [AuditLog](../../domains/audit-logs/audit-log.md) |
| Hangfire job state | [Hangfire](../../engineering/technologies/hangfire.md) |
| Advisory locks (no storage) | [PostgresDistributedLockProvider](../../../src/Infrastructure/Locking/PostgresDistributedLockProvider.cs) |

Note that the outbox, the audit trail and the job store all share the transactional database with the
business data. For the outbox that is the whole point — the event and the business change commit together.
It also means those tables are on the write path of every command, which is why both have retention jobs.

## Two features that are not incidental

* **`xmin`** backs [optimistic concurrency](../../engineering/patterns/optimistic-concurrency.md) with no extra column.
* **Advisory locks** are the default [distributed lock](../../engineering/patterns/distributed-lock.md) provider, so
  locking needs no additional infrastructure.

Moving to another database engine means replacing both. See
[ADR 0004: Optimistic concurrency with PostgreSQL xmin](../../adr/0004-xmin-optimistic-concurrency.md).

## Access

Only through [EF Core](../../engineering/technologies/ef-core.md) from the
[Infrastructure component](../components/infrastructure.md) — see
[Persistence](../cross-cutting/persistence.md). Schema changes go through
[migrations](../../workflows/engineering/add-ef-migration.md).

Anything reading this database outside EF Core does **not** get the soft-delete or tenant query filters and
must apply them itself.
