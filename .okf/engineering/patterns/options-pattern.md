---
type: Pattern
title: "Options Pattern"
description: "Application declares the configuration it needs as an Options class beside its abstraction; Infrastructure binds and validates it; consumers take IOptions<T>, never IConfiguration."
tags: [configuration, options, validation, application, infrastructure]
status: stable
---

# Options Pattern

**Application declares the shape. Infrastructure decides the source.** `XOptions.cs` sits in
`Application/Abstractions/<Concern>/` next to the abstraction that consumes it; the owning Infrastructure
`DependencyInjection.cs` binds it with `services.AddOptions<XOptions>().BindConfiguration("X").ValidateOnStart()`.
Swapping appsettings for a secret manager touches that one line and no Application code.

## How

1. `XOptions.cs` in `Application/Abstractions/<Concern>/` - plain properties, safe defaults.
2. `XOptionsValidator : IValidateOptions<XOptions>` beside it. Validated on start, so a misconfigured
   environment fails immediately with a clear message rather than at first use - which, for something called
   from a background handler, could be silent and much later.
3. Bind and validate in the owning Infrastructure `DependencyInjection.cs`.
4. Add the section to `appsettings.json` with safe defaults, secrets left empty.
5. Consumers inject `IOptions<XOptions>` - `IOptionsMonitor<T>` only when values genuinely change at runtime.

```csharp
internal sealed class TwilioSmsSender(IOptions<SmsOptions> options) : ISmsSender
```

## Never inject IConfiguration

`IConfiguration` gives up type safety, validation and testability, and turns configuration keys into magic
strings scattered across the codebase - a renamed section fails silently at runtime in whichever environment
happened to have it. It is injected nowhere except the composition root that binds options. No architecture
test covers this today; a NetArchTest rule failing any `IConfiguration` constructor parameter outside the
composition root would close the gap.

## Options classes that ship

| Options | Section | For |
|---|---|---|
| [`EmailOptions`](../../../src/Application/Abstractions/Communication/Email/EmailOptions.cs) | `Email` | outbound mail |
| [`SmsOptions`](../../../src/Application/Abstractions/Communication/Sms/SmsOptions.cs) | `Sms` | outbound SMS |
| [`EncryptionOptions`](../../../src/Application/Abstractions/Cryptography/EncryptionOptions.cs) | `Encryption` | key version and legacy keys - [Encryption](encryption.md) |
| [`StorageOptions`](../../../src/Application/Abstractions/Storage/StorageOptions.cs) | `Storage` | file storage roots and entry options |
| [`OutboxOptions`](../../../src/Application/Outbox/OutboxOptions.cs) | `Outbox` | poll interval, batch size, retention |
| Redis | `Redis` | [Caching Tiers](../../architecture/cross-cutting/caching-tiers.md) |

## Secrets

Never commit keys. User secrets or environment variables locally, a secret manager in deployed environments.
