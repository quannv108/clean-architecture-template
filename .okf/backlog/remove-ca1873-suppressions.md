---
type: Task
title: "Remove the CA1873 suppressions once the analyzer is fixed"
description: "A .NET 10 analyzer regression fires on ordinary logger calls; suppressions are in place across Application and Infrastructure."
tags: [backlog, dotnet, analyzers, technical-debt]
status: draft
---

# Remove the CA1873 suppressions once the analyzer is fixed

## Problem

`Microsoft.Extensions.Logging.Abstractions 10.0.3` ships a stricter `CA1873` analyzer that fires on logger
calls passing `DateTime` values, property accesses and other value types. This is a known regression in the
.NET 10 analyzer, not a real defect in the calls.

`#pragma warning disable CA1873` is present in several files across the Application and Infrastructure
layers - find them with:

```bash
grep -rn "CA1873" src/
```

## Action

Remove the suppressions once Microsoft ships a fix in a patch release.

**Do not convert these call sites to `[LoggerMessage]` source generators just to satisfy the analyzer.**
That is a real code change made to appease a temporary bug, and it would outlive the bug.

## How to check

Watch the `Microsoft.Extensions.Logging.Abstractions` release notes. After upgrading, remove one
suppression and build; if CA1873 no longer fires, remove them all.

Related: [Observability](../architecture/cross-cutting/observability.md),
[.NET 10](../engineering/technologies/dotnet-10.md).
