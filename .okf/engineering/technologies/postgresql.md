---
type: Technology
title: "PostgreSQL"
description: "The database, and the source of two features the architecture depends on: xmin row versions and advisory locks."
resource: https://www.postgresql.org
tags: [technology, postgresql, database]
status: stable
---

# PostgreSQL

The application database, run locally as an Aspire container.

Two PostgreSQL-specific features are load-bearing here, which is worth knowing before considering another
provider:

* **`xmin`** - the system column mapped to `Entity.Version` for
  [optimistic concurrency](../patterns/optimistic-concurrency.md). No extra column, nothing to maintain.
* **Advisory locks** - the default
  [distributed lock provider](../../../src/Infrastructure/Locking/PostgresDistributedLockProvider.cs), released automatically
  when the connection closes.

Moving to another provider means replacing both.

Locally, inspect the database with pgweb from the Aspire stack. Integration tests use their own
[Testcontainers](testcontainers.md) instance.
