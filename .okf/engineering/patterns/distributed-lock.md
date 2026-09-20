---
type: Pattern
title: "Distributed Lock"
description: "Coordinate work across application instances with a named lock - and the cases where you should not use one."
resource: src/Application/Abstractions/Locking
tags: [locking, concurrency, distributed, background-jobs]
status: stable
---

# Distributed Lock

## Shape

`lockProvider.CreateLock(name).TryAcquireAsync(timeout, ct)` returns a handle, or `null` when another holder
has it. Dispose the handle to release; process death releases it too (PostgreSQL closes the connection, Redis
expires the key).

```csharp
await using var handle = await lockProvider.CreateLock($"order:process:{id}").TryAcquireAsync(timeout, ct);
if (handle is null) return Result.Failure(OrderErrors.AlreadyBeingProcessed(id));
// critical section - only one instance is here
```

## Timeouts

| Caller | Timeout | Why |
|---|---|---|
| Background job | `TimeSpan.Zero` | Another instance is already doing it - skip, do not queue |
| User request | 5-10 s | Waiting briefly is better than a spurious error |
| Long operation | 5-10 min | Generous, but see the Redis TTL caveat below |

## Naming

`{entity}:{operation}:{id}` - `order:process:123`, `payment:refund:456`, `job:process-outbox` for a global
job. Lowercase, colon-separated, under 100 characters. **No sensitive data** - lock names are logged and
contention diagnostics print them, so prefer an id over an email address. Keep the name stable: changing it
mid-deployment means two versions hold different locks and neither excludes the other.

## Use it for

1. Preventing duplicate execution of a recurring job across instances
2. Rate limiting per resource - one login attempt per user at a time
3. Making a non-idempotent external call safe - payments, partner APIs
4. Leader election
5. Coordinating workers over a shared dataset

## Do not use it for

Concurrent writes to one row (optimistic concurrency is automatic), multi-step atomicity (a transaction), or
high-frequency operations (contention dominates - cache or queue instead). The decision table is in
[Optimistic Concurrency](optimistic-concurrency.md).

## Practicalities

* **Keep critical sections small.** Redis locks expire on a TTL, so a section longer than the TTL can lose
  its lock while still running.
* **Acquire multiple locks in a consistent order**, or accept deadlocks. Better: do not nest locks.
* **Lock per resource, not globally** - `order:process:123`, not `process-all-orders`.
* **Move non-critical work outside the lock.**
* A crashed process releases its lock: PostgreSQL closes the connection, Redis expires the key.

Provider selection is configuration -
[ADR 0007: PostgreSQL advisory locks by default, Redis when configured](../../adr/0007-lock-provider-selection.md). Tests:
`tests/Api.IntegrationTests/Locking/DistributedLockingIntegrationTests.cs`.
