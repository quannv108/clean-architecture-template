---
type: Term
title: "Idempotency"
description: "The property that performing an operation more than once has the same effect as performing it once."
tags: [async, reliability, outbox]
status: stable
---

# Idempotency

Required of every [domain event handler](../../src/Application/Abstractions/Messaging/IQueryHandler.cs) here, because outbox
delivery is [at-least-once](at-least-once-delivery.md) and a retry will call the handler again.

Achieved by checking before acting ("has this already been sent?"), by natural keys that make a duplicate
insert fail harmlessly, or by guards in entity behaviour methods - the `if (Status != Pending) return;` in
[`EmailMessage.MarkAsSent()`](../domains/emails/email-message.md) is exactly this.
