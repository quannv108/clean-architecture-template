---
type: Term
title: "Aggregate Root"
description: "The entity that owns a cluster of objects and is the only entry point for changing them."
tags: [architecture, ddd, domain, consistency]
status: stable
---

# Aggregate Root

An aggregate is a group of objects treated as one unit for consistency; the root is the only member the
outside world may hold a reference to. Changes go through the root's behaviour methods, so its invariants
cannot be bypassed.

Practically: load the root with its children (`Include`), mutate through the root, save once. If two
aggregates must change together in one transaction often, the boundary is probably wrong.

See [Atomic Transaction](../engineering/patterns/atomic-transaction.md).
