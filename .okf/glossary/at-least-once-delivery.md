---
type: Term
title: "At-Least-Once Delivery"
description: "A guarantee that a message is delivered, possibly more than once."
tags: [async, reliability, outbox]
status: stable
---

# At-Least-Once Delivery

The outbox's delivery guarantee. A message will not be lost, but a failure between dispatching and marking
it processed means it is dispatched again.

The alternative, exactly-once, is not available without cooperation from every downstream system.
[Idempotent](idempotency.md) handlers turn at-least-once into effectively-once, which is the practical
answer.
