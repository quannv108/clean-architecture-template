---
type: Technology
title: ".NET 11"
description: "The target framework, pinned by global.json."
resource: https://dotnet.microsoft.com/download
tags: [technology, dotnet, runtime, sdk]
status: stable
---

# .NET 11

The solution targets .NET 11 (`net11.0` in `api/Directory.Build.props`). The SDK version is pinned in
`api/global.json`, so everyone builds with the same toolchain.

Features this codebase relies on: `Guid.CreateVersion7()` for time-ordered ids, minimal APIs with typed
results, `HybridCache`, and the built-in rate limiting middleware.

Package versions are centrally managed in `Directory.Packages.props`; never pin a version in a `.csproj`.
Packages released with the runtime (`Microsoft.Extensions.*`, `Microsoft.AspNetCore.*`,
`Microsoft.EntityFrameworkCore*`) stay on the same major as the target framework — mixing majors runs two
`LoggerMessage` source generators and fails the build with `CS0757`.

## Known issue: prerelease PostgreSQL provider

EF Core 11.0.0 is GA, but `Npgsql.EntityFrameworkCore.PostgreSQL` is still `11.0.0-rc.3` and
`EFCore.NamingConventions` has no 11.x build. Until both ship GA:

* `Infrastructure.csproj` references `Microsoft.EntityFrameworkCore` and `.Relational` directly, so 11.0.0 wins
  over the provider's exact `[11.0.0-rc.3]` pin.
* `Directory.Build.props` suppresses `NU1608` (package resolved outside a dependency's declared range).

When they ship, bump both packages, then delete the two direct references and the `NU1608` suppression.
