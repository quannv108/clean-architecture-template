---
type: Term
title: "Test-Driven Development"
description: "Writing or adjusting the test before the implementation."
tags: [testing, process]
status: stable
---

# Test-Driven Development

The stated workflow here for class enhancements: adjust or write the test first, implement, confirm the
whole suite passes.

The practical benefit in this codebase is that writing the test first forces you to decide what
[`Result`](../../src/SharedKernel/Result.cs) each path returns, which is exactly the design question a handler has to
answer.

See [Build and Test](../workflows/engineering/build-and-test.md).
