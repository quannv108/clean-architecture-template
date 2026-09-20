---
type: Term
title: "Outbox"
description: "A table of pending messages written inside the business transaction and dispatched afterwards."
tags: [async, outbox, reliability]
status: stable
---

# Outbox

The transactional outbox pattern. Writing the intent to publish in the same transaction as the business
change makes the two atomic: either both happened or neither did. Delivery afterwards is at-least-once.

See [Outbox Pattern](../engineering/patterns/outbox-pattern.md) and
[Outbox](../domains/outbox/outbox.md).
