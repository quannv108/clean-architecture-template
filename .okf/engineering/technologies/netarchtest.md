---
type: Technology
title: "NetArchTest"
description: "The library the architecture rules are written with."
resource: https://github.com/BenMorris/NetArchTest
tags: [technology, testing, architecture]
status: stable
---

# NetArchTest

Expresses architecture rules as ordinary unit tests over assembly metadata - layer dependencies,
visibility, naming, inheritance.

It is what turns this bundle's [Constraints](../constraints.md) from documentation into build failures.
A rule that only exists in prose gets broken; a rule that fails the build gets fixed at the moment it is
introduced.

All the rules live in [`ArchitectureTests`](../../architecture/delivery/test-architecture.md).
