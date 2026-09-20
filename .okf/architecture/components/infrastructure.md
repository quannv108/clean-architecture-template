---
type: Component
title: Infrastructure Layer
description: Implementations of Application abstractions - EF Core and PostgreSQL, cryptography, messaging adapters, lock providers, storage, hosted services.
resource: src/Infrastructure
tags: [layer, infrastructure, ef-core, adapters]
status: stable
---

# Infrastructure Layer

`src/Infrastructure` references Application, Domain and SharedKernel. It supplies the concrete thing behind
every abstraction the Application layer declared.

## What lives here

| Folder | Holds |
|---|---|
| `Database/` | [`ApplicationDbContext`](../../../src/Infrastructure/Database/ApplicationDbContext.cs), entity configurations, converters, interceptors, migrations, seeding |
| `Authentication/` | [`UserContext`](../../../src/Infrastructure/Authentication/UserContext.cs), claims extensions |
| `Cryptography/` | [`Encryptor`](../../../src/Infrastructure/Cryptography/Encryptor.cs), [`Hasher`](../../../src/Infrastructure/Cryptography/Hasher.cs) |
| `Communication/Email` | [`SesEmailSender`](../../../src/Infrastructure/Communication/Email/SesEmailSender.cs), [`DummyEmailSender`](../../../src/Infrastructure/Communication/Email/DummyEmailSender.cs) |
| `Communication/Sms` | [`TwilioSmsSender`](../../../src/Infrastructure/Communication/Sms/TwilioSmsSender.cs), [`DummySmsSender`](../../../src/Infrastructure/Communication/Sms/DummySmsSender.cs) |
| `Locking/` | [PostgreSQL](../../../src/Infrastructure/Locking/PostgresDistributedLockProvider.cs) and [Redis](../../../src/Infrastructure/Locking/RedisDistributedLockProvider.cs) lock providers |
| `Storage/` | [`StorageFactory`](../../../src/Infrastructure/Storage/StorageFactory.cs), [`SystemFileStorage`](../../../src/Infrastructure/Storage/SystemFileStorage.cs) |
| `BackgroundJobs/` | [`IBackgroundJob`](../../../src/Application/Abstractions/BackgroundJobs/IBackgroundJob.cs) adapters and the recurring job configurator |
| `Outbox/` | [`OutboxMessageHostedService`](../../../src/Infrastructure/Outbox/OutboxMessageHostedService.cs), [`OutboxSignal`](../../../src/Infrastructure/Outbox/OutboxSignal.cs) |
| `DomainEvents/` | [`DomainEventsDispatcher`](../../../src/Infrastructure/DomainEvents/DomainEventsDispatcher.cs) |
| `Time/` | [`DateTimeProvider`](../../../src/Infrastructure/Time/DateTimeProvider.cs) |

## The rules that define this layer

* **Everything is `internal sealed`** except interfaces, extension classes, configuration types, constants
  and enums. Enforced by `tests/ArchitectureTests/Infrastructure/InfrastructureTests.cs`. See
  [Visibility](../../engineering/conventions/visibility.md).
* **No business logic.** An adapter translates between an Application abstraction and an SDK. If you find
  yourself writing a rule here, it belongs in Domain or Application.
* **Options are bound here, not shaped here.**
  `services.AddOptions<SmsOptions>().BindConfiguration("Sms")` lives in Infrastructure DI; `SmsOptions`
  itself lives in Application.
* **Hosted services live here.** `IHostedService` / `BackgroundService` implementations are infrastructure
  concerns and are registered in `Infrastructure/DependencyInjection.cs` - never in Web.Api.
* **Provider selection is configuration-driven.** Empty `Redis:ConnectionString` selects the PostgreSQL lock
  provider and an L1-only cache; a populated one selects Redis for both. See
  [Caching Tiers](../cross-cutting/caching-tiers.md) and [Distributed Lock](../../engineering/patterns/distributed-lock.md).

## Migrations

EF Core migrations live in `src/Infrastructure/Database/Migrations` and must be `internal partial`. The exact
command set, including the two flags that are easy to forget, is in
[Add an EF Core Migration](../../workflows/engineering/add-ef-migration.md).
