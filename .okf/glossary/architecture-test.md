---
type: Term
title: "Architecture Test"
description: "A unit test asserting a structural rule about the codebase."
tags: [testing, architecture, netarchtest]
status: stable
---

# Architecture Test

Written with [NetArchTest](../engineering/technologies/netarchtest.md) over assembly metadata: layer dependencies,
visibility, naming, inheritance, handler shape.

They turn this bundle's [Constraints](../engineering/constraints.md) from prose into build failures. A rule only
documented gets broken; a rule that fails the build gets fixed at the moment it is introduced.

**Run them before completing any work.** A failure is a design error, not a test to loosen. See
[ArchitectureTests](../architecture/delivery/test-architecture.md).
