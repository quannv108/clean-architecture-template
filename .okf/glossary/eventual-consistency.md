---
type: Term
title: "Eventual Consistency"
description: "State that becomes consistent after a delay rather than immediately."
tags: [async, outbox, consistency]
status: stable
---

# Eventual Consistency

Everything downstream of a [domain event](domain-event.md) here is eventually consistent: the command
commits and returns, and the side effect happens some time later.

Consequences to design for: a client cannot read its own event's side effect immediately; integration tests
must call `WaitForOutboxMessagesAsync()`; a user-facing feature that must be instant should not be built on
an event.

See [ADR 0003: Dispatch domain events asynchronously via the Outbox](../adr/0003-async-domain-events-via-outbox.md).
