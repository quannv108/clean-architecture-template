---
type: Term
title: "Leader Election"
description: "Choosing one instance to perform work that must happen only once."
tags: [async, distributed, locking, background-jobs]
status: stable
---

# Leader Election

Achieved here with a [distributed lock](../engineering/patterns/distributed-lock.md) rather than a dedicated protocol:
whichever instance acquires the named lock is the leader for that piece of work, and the others skip.

Use `TimeSpan.Zero` so non-winners skip immediately instead of queueing to do work that has already been
done.
