---
type: Term
title: "Optimistic Concurrency"
description: "Detecting a conflicting concurrent write at save time rather than preventing it with a lock."
tags: [data, concurrency, ef-core]
status: stable
---

# Optimistic Concurrency

Assume conflicts are rare, check at save. Here the check is PostgreSQL's [`xmin`](xmin.md) row version: EF
adds `WHERE xmin = @version`, and zero affected rows means somebody else won.

Costs nothing until a conflict happens, and holds no lock between load and save - unlike
[pessimistic locking](pessimistic-locking.md).

See [Optimistic Concurrency](../engineering/patterns/optimistic-concurrency.md).
