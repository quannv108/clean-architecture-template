---
type: Term
title: "Unit of Work"
description: "A boundary within which changes are tracked and committed together."
tags: [data, persistence, ef-core, transactions]
status: stable
---

# Unit of Work

EF Core's `DbContext` **is** a unit of work. One `SaveChangesAsync` commits every pending change - entities
and the [`OutboxMessage`](../domains/outbox/outbox-message.md) rows created by domain events - in one transaction.

Hence the rule: **one `SaveChangesAsync` per handler**. Calling it twice makes the operation non-atomic.

See [Atomic Transaction](../engineering/patterns/atomic-transaction.md).
