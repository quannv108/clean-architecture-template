---
type: Term
title: "Domain Event"
description: "A record of something significant that happened in the domain, dispatched to handlers asynchronously."
tags: [async, domain-events, ddd]
status: stable
---

# Domain Event

An immutable positional record implementing [`IDomainEvent`](../../src/SharedKernel/IDomainEvent.cs), raised by an
entity in a behaviour method, carrying identifiers rather than entities.

Here events are **never dispatched in memory**. They are persisted as
[`OutboxMessage`](../domains/outbox/outbox-message.md) rows in the same transaction and delivered afterwards, which
is what makes them reliable and what makes everything downstream
[eventually consistent](eventual-consistency.md).

See [Domain Event](../engineering/patterns/domain-event.md).
