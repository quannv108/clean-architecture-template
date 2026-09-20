---
type: Term
title: "AAA Pattern"
description: "Arrange, Act, Assert - the three-part structure of a unit test."
tags: [testing, unit-tests]
status: stable
---

# AAA Pattern

Arrange the substitutes and inputs, act by calling the method once, assert on the outcome.

The rule that carries the value is **one act per test**. A test with three acts cannot tell you which one
broke, and its name cannot honestly describe what it covers.

Required by [`Application.UnitTests`](../architecture/delivery/test-architecture.md).
