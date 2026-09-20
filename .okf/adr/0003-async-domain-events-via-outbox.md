---
type: ADR
title: "ADR 0003: Dispatch domain events asynchronously via the Outbox"
description: "Persist raised domain events in the business transaction and dispatch them from a background service, rather than in-memory during SaveChanges."
tags: [adr, domain-events, outbox, reliability, async]
status: stable
generated:
  by: anthropic/claude-opus-5
  at: 2026-09-19T00:00:00Z
---

# ADR 0003: Dispatch domain events asynchronously via the Outbox

**Status:** Accepted - reconstructed on 2026-09-19 from the codebase and its previous `docs/`
tree. The decision was already in force; this record was written afterwards, so the context and
alternatives are inferred. Correct them if you were there.

## Context

The common approach dispatches domain events in-memory during or just after `SaveChangesAsync`. It is
simple and the side effect is immediate.

It also has two failures that only appear in production. If the process dies between the commit and the
dispatch, the event is gone with no trace. And if a subscriber is slow or failing - sending an email,
calling a partner API - the user's request pays for it, and a subscriber's outage becomes the endpoint's
outage.

## Decision

Events raised through [`Entity.Raise`](../../src/SharedKernel/Entity.cs) are written as
[`OutboxMessage`](../domains/outbox/outbox-message.md) rows **in the same transaction** as the business data.
[`OutboxMessageHostedService`](../../src/Infrastructure/Outbox/OutboxMessageHostedService.cs) polls, and
[`OutboxMessageProcessor`](../../src/Application/Outbox) dispatches to
[`IDomainEventHandler<T>`](../../src/Application/Abstractions/Messaging/IQueryHandler.cs).

[`OutboxSignal`](../../src/Infrastructure/Outbox/OutboxSignal.cs) wakes the poller immediately after a save, so latency is low
in the common case without the correctness depending on it.

## Consequences

**Good.** An event is never lost - commit and intent-to-publish are atomic. Subscriber latency and failure
are invisible to the caller. Failures are visible as rows with errors and retryable.

**Costly.** Everything downstream of an event is eventually consistent. A handler cannot influence the
response. Delivery is at-least-once, so **every handler must be idempotent**. Ordering across different
events is not guaranteed. Integration tests must call `WaitForOutboxMessagesAsync()` before asserting a side
effect. The outbox table is on the write path of every event-raising command and needs pruning
([`OutboxMessageCleanupJob`](../../src/Application/Outbox/OutboxMessageCleanupJob.cs)) and monitoring.

**Alternatives rejected.** In-memory dispatch - loses events, couples latency. A message broker - correct at
larger scale, but the outbox is what you need *before* the broker anyway, and this template should run with
PostgreSQL alone.

Mechanism: [Outbox Pattern](../engineering/patterns/outbox-pattern.md).
