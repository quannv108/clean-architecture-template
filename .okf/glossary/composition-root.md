---
type: Term
title: "Composition Root"
description: "The single place where the object graph is wired up."
tags: [application, di, startup]
status: stable
---

# Composition Root

Here it is split per layer - `Application/DependencyInjection.cs`,
`Infrastructure/DependencyInjection.cs`, `Web.Api/DependencyInjection.cs` - each owning its own
registrations and called from `Program.cs`.

It is also the **only** place `IConfiguration` legitimately appears: it binds options classes, and
everything else takes `IOptions<T>`. See [Options Pattern](../engineering/patterns/options-pattern.md).
