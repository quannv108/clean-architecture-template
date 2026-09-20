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

Package versions are centrally managed in `Directory.Packages.props`; never pin a version in a `.csproj`.
