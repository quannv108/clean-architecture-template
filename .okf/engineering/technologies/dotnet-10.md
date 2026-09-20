---
type: Technology
title: ".NET 10"
description: "The target framework, pinned by global.json."
resource: https://dotnet.microsoft.com/download
tags: [technology, dotnet, runtime, sdk]
status: stable
---

# .NET 10

The solution targets .NET 10. The SDK version is pinned in `global.json`, so everyone builds with the same
toolchain.

Features this codebase relies on: `Guid.CreateVersion7()` for time-ordered ids, minimal APIs with typed
results, `HybridCache`, and the built-in rate limiting middleware.

**Known issue:** `Microsoft.Extensions.Logging.Abstractions 10.0.3` ships a stricter `CA1873` analyzer that
fires on ordinary logger calls. It is suppressed in several files - see
[Remove the CA1873 suppressions once the analyzer is fixed](../../backlog/remove-ca1873-suppressions.md).

Package versions are centrally managed in `Directory.Packages.props`; never pin a version in a `.csproj`.
