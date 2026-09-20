---
type: Term
title: "Shared Kernel"
description: "The dependency-free set of types every layer may use."
tags: [architecture, ddd, sharedkernel]
status: stable
---

# Shared Kernel

Two things only: DDD primitives at the root ([`Entity`](../../src/SharedKernel/Entity.cs),
[`Result`](../../src/SharedKernel/Result.cs), [`ValueObject`](../../src/SharedKernel/ValueObject.cs), ...) and cross-slice
value objects in named subfolders.

Everything can see it, which is exactly why what goes in must be kept small: a type placed here becomes
impossible to keep out of the wrong layer. Infrastructure interface contracts never belong here - they go
in `Application/Abstractions/`.

See [SharedKernel Layer](../architecture/components/shared-kernel.md).
