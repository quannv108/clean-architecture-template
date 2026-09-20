---
type: Technology
title: "Shouldly"
description: "The assertion library - used instead of FluentAssertions, enforced by architecture tests."
resource: https://docs.shouldly.org
tags: [technology, testing, assertions]
status: stable
---

# Shouldly

The only assertion library in this solution. `tests/ArchitectureTests/Testing/TestingStandardsTests.cs`
fails the build if FluentAssertions appears.

```csharp
result.IsSuccess.ShouldBeTrue();
result.Error.Code.ShouldBe("Order.NotFound");
```

Assert on the [`Error.Code`](../../../src/SharedKernel/Error.cs), not merely that the result failed - the code is the
API contract, and a test that only checks `IsFailure` passes when the wrong error is returned.
