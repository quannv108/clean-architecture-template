---
type: Term
title: "Code Coverage"
description: "The proportion of code executed by the test suite."
tags: [testing, quality, metrics]
status: stable
---

# Code Coverage

Target here: **70%+ for the Application layer**, reported by CI on every pull request.

Coverage is a smoke detector, not a goal. A handler at 100% with no assertion on its
`Result.Failure` branches proves only that the lines ran. Cover each failure path and assert on the
[`Error.Code`](../../src/SharedKernel/Error.cs).

See [Generate a Coverage Report](../workflows/engineering/generate-coverage.md).
