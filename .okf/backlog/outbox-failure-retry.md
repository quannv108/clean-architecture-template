---
type: Task
title: "Improve handling of failed outbox messages"
description: "Persist failed messages in a distinct status and add a background service that retries them with back-off for transient failures."
tags: [backlog, outbox, reliability, retry]
status: draft
---

# Improve handling of failed outbox messages

**Source:** carried over from the previous `docs/PendingTasks.md`.

## Problem

When [`OutboxMessageProcessor`](../../src/Application/Outbox) fails to dispatch a message, the
error is recorded and the message stays unprocessed. Transient failures - a network blip, a provider being
briefly down - are then retried at the same cadence as everything else, and a permanently broken message is
retried forever alongside them.

There is no distinction between "will succeed shortly" and "will never succeed".

## Proposed change

* A distinct status for failed messages, separate from pending.
* A background service that retries failed messages with exponential back-off.
* A retry ceiling, after which a message is parked as dead-lettered rather than retried.
* Surface parked messages so somebody is told, rather than leaving them for
  [`GetOutboxTypeErrorsQuery`](../../src/Application/Outbox/GetOutboxTypeErrorsQuery.cs) to be noticed by chance.

## Considerations

* Handlers must be idempotent anyway, so retrying is safe - but retry count should be capped per message,
  not per type.
* A poison message currently consumes a slot in every batch; separating statuses fixes that too.
* Ordering guarantees, already weak, weaken further with back-off. Say so explicitly if any handler
  assumes order.

Related: [Outbox Pattern](../engineering/patterns/outbox-pattern.md).
