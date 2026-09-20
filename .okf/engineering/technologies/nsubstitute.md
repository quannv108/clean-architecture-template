---
type: Technology
title: "NSubstitute"
description: "The mocking library - used instead of Moq, enforced by architecture tests."
resource: https://nsubstitute.github.io
tags: [technology, testing, mocking]
status: stable
---

# NSubstitute

The only mocking library in this solution. `tests/ArchitectureTests/Testing/TestingStandardsTests.cs` fails
the build if Moq appears.

```csharp
var clock = Substitute.For<IDateTimeProvider>();
clock.UtcNow.Returns(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
```

`DbSet` is mocked with `BuildMock()` from **MockQueryable.NSubstitute**.

One mocking library means every test file starts the same way and no reviewer context-switches. See
[ADR 0014: NSubstitute, Shouldly and Testcontainers as the test stack](../../adr/0014-nsubstitute-shouldly-testcontainers.md).
