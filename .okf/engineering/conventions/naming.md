---
type: Convention
title: "Naming and Placement"
description: "Where every kind of file lives and what it is called - one table, one row per kind."
tags: [naming, placement, files, layers]
status: stable
---

# Naming and Placement

One row per kind of thing. Find the row, and the folder, file name and class name follow. Several rows are
asserted by the [architecture tests](../constraints.md) - a wrong name or folder fails the build, not just
review.

| What | Where | Name | Example |
|---|---|---|---|
| DDD primitives (`Entity`, `Result`, `EncryptedString`, ...) | `SharedKernel/` root | as is | `Entity.cs` |
| Cross-slice value objects and their errors | `SharedKernel/<Concept>/` | `<Concept>.cs`, `<Concept>Errors.cs` | `PhoneNumbers/PhoneNumber.cs` |
| Entity | `Domain/<Feature>/` | `<Feature>.cs` | `Orders/Order.cs` |
| Domain errors | `Domain/<Feature>/` | `<Feature>Errors.cs` - `public static` factories | `OrderErrors.cs` |
| Slice-local value objects | `Domain/<Feature>/` | `<Concept>.cs` | |
| Domain event | `Domain/<Feature>/` | `<Event>DomainEvent.cs` - positional record | `OrderConfirmedDomainEvent.cs` |
| Infrastructure interface contracts | `Application/Abstractions/<Concern>/` | `I<Thing>` | `Communication/IEmailSender.cs` |
| Options class and its validator | `Application/Abstractions/<Concern>/` | `<Concern>Options.cs`, `<Concern>OptionsValidator.cs` | `SmsOptions.cs` |
| Command + handler | `Application/<Feature>/` | `<Operation>Command.cs` holding `<Operation>Command` and `<Operation>CommandHandler` | `CreateOrderCommand.cs` |
| Query + handler + DTOs | `Application/<Feature>/` | `<Operation>Query.cs` holding `<Operation>Query`, `<Operation>QueryHandler`, `<Feature>Response` | `GetOrderByIdQuery.cs` |
| Cached repository | `Application/<Feature>/Data/` | `I<Feature>CachedRepository.cs`, `<Feature>CachedRepository.cs` | |
| Domain event handler | `Application/<Feature>/Events/` | `<Event>DomainEventHandler.cs` | |
| Job logic (does **not** implement `IBackgroundJob`) | `Application/<Feature>/` | `<Task>BackgroundJob.cs` | `OutboxMessageCleanupJob.cs` |
| Permissions | `Application/<Feature>/` | `<Feature>PermissionsConstants.cs` | |
| Job-runner adapter (implements `IBackgroundJob`) | `Infrastructure/BackgroundJobs/` | `<Runner>BackgroundJob.cs` | `HangfireBackgroundJob.cs` |
| Hosted service | `Infrastructure/<Feature>/` | `<Thing>HostedService.cs` | `Outbox/OutboxMessageHostedService.cs` |
| Provider implementation | `Infrastructure/<Concern>/` | interface name without `I`, `internal sealed` | `TwilioSmsSender.cs` |
| EF configuration | `Infrastructure/Database/Configuration/<Feature>/` | `<Entity>Configuration.cs` | |
| Seeder | `Infrastructure/Database/Seeder/<Feature>/` | `<Feature>Seeder.cs` | |
| Endpoint | `Web.Api/Endpoints/<Feature>/` | `<Operation>.cs` - **not** `<Operation>Endpoint.cs` | `CreateOrder.cs` |
| Middleware, result mapping, exception handling | `Web.Api/` | | |
| Unit tests | `tests/Application.UnitTests/<Feature>/` | `<Operation>HandlerTests.cs` | |

## Interface or implementation - which layer?

The interface goes where it is **consumed** (`Application/Abstractions/<Concern>/`), the implementation where
it is **satisfied** (`Infrastructure/<Concern>/`). Never put an infrastructure interface in SharedKernel:
SharedKernel is visible to Domain, so an entity could take a dependency on sending email - exactly what the
layering prevents - and the architecture tests would not catch it because the reference is legal.
`Application/Abstractions/` is invisible to Domain, so the compiler enforces it.

```
Application/Abstractions/
  Authentication/   IUserContext
  BackgroundJobs/   IBackgroundJob
  Communication/    IEmailSender, ISmsSender (+ Options)
  Cryptography/     IEncryptor, IHasher (+ EncryptionOptions)
  Data/             IApplicationDbContext, IReadOnlyApplicationDbContext, IEntityDeleter
  DomainEvents/     IDomainEventsDispatcher
  Locking/          IDistributedLockProvider
  Messaging/        ICommand, ICommandHandler, IQuery, IQueryHandler
  Storage/          IStorage, IStorageFactory (+ Options)
  Time/             IDateTimeProvider
  Validation/       custom validation attributes
```

Adding one: interface in `Application/Abstractions/<Concern>/`; options class and validator beside it if it
needs configuration ([Options Pattern](../patterns/options-pattern.md)); `internal sealed` implementation in
`Infrastructure/<Concern>/`; registration in the owning Infrastructure `DependencyInjection.cs`.

## The three that get it wrong most often

* **Endpoints are `<Operation>.cs`.** The folder already says it is an endpoint.
* **A class ending `BackgroundJob` that does not implement `IBackgroundJob` is job logic**, in Application -
  [Background Processing](../../architecture/cross-cutting/background-processing.md).
* **Two slices need the same value object** - it moves to `SharedKernel/<Concept>/`. It does not stay in one
  slice's folder with the other referencing it; that is the coupling slices exist to prevent.

## General

* One public type per file, named after the file.
* Interfaces prefixed `I`; the implementation drops the prefix (`IUserContext` -> `UserContext`).
* `<Feature>` is plural when it names a slice folder (`Orders/`), singular when it names the entity
  (`Order.cs`).
* Names come from the business, not the framework: `ConfirmOrder`, not `UpdateOrderStatusToConfirmed`.

Related: [SharedKernel Layer](../../architecture/components/shared-kernel.md),
[Error Codes](error-codes.md), [Record Syntax](record-syntax.md), [Visibility](visibility.md).
