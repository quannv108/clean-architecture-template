---
type: Task
title: "Replace outbox polling with channel-based notification"
description: "Stop polling OutboxMessage on a fixed interval; use a Channel to notify the processor and back off polling when idle."
tags: [backlog, outbox, performance, database]
status: draft
---

# Replace outbox polling with channel-based notification

**Source:** carried over from the previous `docs/PendingTasks.md`.

## Problem

[`OutboxMessageHostedService`](../../src/Infrastructure/Outbox/OutboxMessageHostedService.cs) polls on a fixed interval.
That is a direct trade between event latency and idle database load: a short interval means a query every
few seconds forever, most of them returning nothing; a long interval means side effects lag.

[`OutboxSignal`](../../src/Infrastructure/Outbox/OutboxSignal.cs) already softens this in-process, but the timer still runs
at full rate when nothing is happening.

## Proposed change

Use a `Channel` to notify the processor when messages are written, and **slow the polling down when idle**
rather than removing it. Polling stays as the correctness mechanism - messages written by another instance,
or written just before a restart, are only found by the timer.

## Considerations

* The signal is in-process; in a multi-instance deployment each instance only learns about its own writes.
  PostgreSQL `LISTEN`/`NOTIFY` would cover the cross-instance case, at the cost of a held connection.
* Back-off needs a ceiling, or a message that arrives during a quiet period waits for the longest interval.
* Any change here must keep `WaitForOutboxMessagesAsync()` working for integration tests.

Related: [Outbox Pattern](../engineering/patterns/outbox-pattern.md),
[OutboxOptions](../../src/Application/Outbox/OutboxOptions.cs).
