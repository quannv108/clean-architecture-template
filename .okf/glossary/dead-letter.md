---
type: Term
title: "Dead Letter"
description: "A message parked after repeated failed delivery, so it stops consuming retry capacity."
tags: [async, outbox, reliability]
status: stable
---

# Dead Letter

**Not currently implemented here.** A failed [`OutboxMessage`](../domains/outbox/outbox-message.md) records its
error and is retried indefinitely, so a permanently broken message competes with healthy ones forever.

Adding a retry ceiling and a parked state is
[Improve handling of failed outbox messages](../backlog/outbox-failure-retry.md).
