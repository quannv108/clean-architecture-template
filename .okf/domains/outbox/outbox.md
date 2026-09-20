---
type: Domain Slice
title: "Outbox"
description: "The slice that makes domain event delivery reliable: events persisted with the business transaction and dispatched afterwards."
resource: src/Domain/Outbox
tags: [domain, outbox, domain-events, infrastructure-slice]
status: stable
---

# Outbox

The Outbox is what makes "raise an event and something happens later" trustworthy. It is a domain slice by
file layout, but it is infrastructure by purpose - almost every product built on this template keeps it.

## Across the layers

| Layer | Files |
|---|---|
| Domain | [`OutboxMessage`](outbox-message.md), [`OutboxMessageErrors`](outbox-message-errors.md) |
| Application | [`OutboxMessageProcessor`](../../../src/Application/Outbox), [`IOutboxMessageProcessor`](../../../src/Application/Outbox/IOutboxMessageProcessor.cs), [`OutboxOptions`](../../../src/Application/Outbox/OutboxOptions.cs), [cleanup command](../../../src/Application/Outbox/CleanupProcessedOutboxMessagesCommand.cs) and [job](../../../src/Application/Outbox/OutboxMessageCleanupJob.cs), [stats](../../../src/Application/Outbox/GetOutboxStatsQuery.cs) and [error](../../../src/Application/Outbox/GetOutboxTypeErrorsQuery.cs) queries |
| Infrastructure | [`OutboxMessageHostedService`](../../../src/Infrastructure/Outbox/OutboxMessageHostedService.cs), [`OutboxSignal`](../../../src/Infrastructure/Outbox/OutboxSignal.cs), [`OutboxMessageConfiguration`](../../../src/Infrastructure/Database/Configuration/Outbox/OutboxMessageConfiguration.cs) |
| Web.Api | [Outbox dev pages](../../../src/Web.Api/Pages/Dev) |

## How it behaves

Writing the message inside the business transaction is the whole trick: either both the entity change and
the intent to publish are committed, or neither is. Delivery afterwards is at-least-once, so handlers must
be idempotent. Full mechanism: [Outbox Pattern](../../engineering/patterns/outbox-pattern.md); decision:
[ADR 0003: Dispatch domain events asynchronously via the Outbox](../../adr/0003-async-domain-events-via-outbox.md).

## Operating it

Watch pending count and pending age - see [Observability](../../architecture/cross-cutting/observability.md). A rising
backlog means side effects across the whole system are silently not happening. Known gaps:
[Backlog](../../backlog/index.md).

## Files in this slice

* [OutboxMessage](outbox-message.md) - The persisted form of a raised domain event, written in the same transaction as the business data.
* [OutboxMessageErrors](outbox-message-errors.md) - Domain error factories for outbox message failures.

Implementation: [`src/Application/Outbox`](../../../src/Application/Outbox) and [`src/Infrastructure/Outbox`](../../../src/Infrastructure/Outbox); mechanism in [Outbox Pattern](../../engineering/patterns/outbox-pattern.md).
