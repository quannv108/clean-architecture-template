---
type: Term
title: "Fire-and-Forget"
description: "Starting work without awaiting it or acting on its outcome."
tags: [async, patterns]
status: stable
---

# Fire-and-Forget

Used deliberately for [audit logging](../engineering/patterns/audit-logging.md): the write happens after the response is
produced, and a failure is logged but never fails the request.

The trade-off is explicit - auditing cannot become an availability risk, and in exchange **the audit trail
is not guaranteed complete**. If a regulation requires completeness, the write has to move inside the
transaction.

It is not a general technique. Everywhere else, work that must happen goes through the
[Outbox](../engineering/patterns/outbox-pattern.md).
