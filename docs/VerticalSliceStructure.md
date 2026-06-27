# Vertical Slice Structure

Complete analysis of all features and their organization across layers in this Clean Architecture project.

## Standard Folder Structure

Every feature follows this pattern (scaled to complexity):

```
<Feature>/
├── Domain/<Feature>/
│   ├── <Feature>.cs                    # Main entity
│   ├── <Feature>Errors.cs              # Domain errors (static class)
│   ├── <Related>.cs                    # Related entities
│   └── *DomainEvent.cs                 # Domain events
│
├── Application/<Feature>/
│   │   ├── <Operation>CommandHandler.cs
│   │   ├── <Operation>QueryHandler.cs
│   ├── Data/                           # CachedRepository (reads only)
│   │   ├── I<Feature>CachedRepository.cs # interface, implement, dto
│   ├── Events/                         # Domain event handlers
│   └── <Feature>PermissionsConstants.cs
│
├── Infrastructure/Database/Configuration/<Feature>/
│   ├── <Feature>Configuration.cs       # EF Core config
│   └── <Feature>Seeder.cs             # Seeding (optional)
│
└── Web.Api/Endpoints/<Feature>/
    ├── <Operation>.cs                  # IEndpoint implementation
    └── <SubFeature>/                   # Nested (for complex features)
        └── <SubOperation>.cs
```

## SharedKernel Placement Rules

SharedKernel has two distinct categories. Putting something in the wrong one creates cross-slice coupling or misleads newcomers about where interface contracts live.

### Category 1 — DDD Building Blocks (root level)

Things every layer inherits from, implements, or uses as a property type. No domain-specific meaning. No external side effects.

| File | Role |
|------|------|
| `Entity.cs` | Base for all domain entities |
| `AuditedEntity.cs` | Entity with audit fields |
| `ValueObject.cs` | Base for value objects |
| `Result.cs` / `Error.cs` | Discriminated union for operation outcomes |
| `IDomainEvent.cs` | Marker interface for domain events |
| `EncryptedString.cs` | Property type — encrypt/decrypt handled by Infrastructure |

**Rule:** If a class or interface belongs here, any layer can use it and it carries no domain-specific meaning.

### Category 2 — Cross-Slice Value Objects (named concept subfolders)

Value objects shared by more than one domain slice. They cannot live in `Domain/<Feature>/` because other slices would depend on that feature's folder — cross-slice coupling. Each concept gets its own named subfolder.

```
SharedKernel/
└── PhoneNumbers/
    ├── PhoneNumber.cs
    └── PhoneNumberErrors.cs
```

Future examples: `Money/`, `Address/`, `EmailAddress/`.

**Rule:** More than one slice needs this value object → `SharedKernel/<Concept>/`. Only one slice needs it → `Domain/<Feature>/`.

### Where interface contracts go

All infrastructure interface contracts (`IEmailSender`, `ISmsSender`, `IDateTimeProvider`, `IBackgroundJob`, etc.) belong in `Application/Abstractions/`, never in SharedKernel. Domain must not be able to call infrastructure services directly; the layer enforcement (Architecture tests) guarantees this as long as interfaces stay in Application.

### Options classes (configuration)

Options classes (read from `appsettings.json` or other configuration sources) are **defined in Application layer** alongside the abstractions that consume them. Infrastructure is responsible only for wiring them up via `services.AddOptions<T>().BindConfiguration(...)` or equivalent.

```
Application/Abstractions/Communication/Sms/
├── ISmsSender.cs          # interface
└── SmsOptions.cs          # options class — defined here

Infrastructure/Communication/Sms/
├── TwilioSmsSender.cs     # implementation
└── DependencyInjection.cs # services.AddOptions<SmsOptions>().BindConfiguration("Sms")
```

**Rule:** Application defines the shape of configuration it needs; Infrastructure decides where to load it from (appsettings, environment variables, secrets manager, etc.). This keeps Application free of infrastructure concerns while allowing Infrastructure to swap configuration sources without touching Application code.

### Hosted services

`IHostedService` / `BackgroundService` implementations belong in **Infrastructure**, never in Application or Web.Api. They are infrastructure concerns: polling databases, processing queues, scheduling background work. Register them in Infrastructure's `DependencyInjection.cs`:

```
Infrastructure/Outbox/
└── OutboxMessageHostedService.cs   # BackgroundService implementation

Infrastructure/DependencyInjection.cs
    services.AddHostedService<OutboxMessageHostedService>();
```

**Rule:** Application defines the *contract* for background work (e.g., `IBackgroundJob`) if one is needed; Infrastructure provides the hosted service that drives that work. Web.Api's `DependencyInjection.cs` should not register hosted services directly.

### Background job classes

There are two distinct concepts — do not confuse them:

| Concept | Where | Examples |
|---------|-------|---------|
| **Job logic class** — the actual work, calls command handlers | `Application/<Feature>/` | `OutboxMessageCleanupJob`, `DeleteOldAuditLogsBackgroundJob` |
| **Library adapter** — wraps the job-runner SDK (`IBackgroundJob` impl) | `Infrastructure/BackgroundJobs/` | `HangfireBackgroundJob`, `SimpleBackgroundJob` |

**Job logic classes** live in `Application/<Feature>/` alongside the handlers they orchestrate. They contain an `ExecuteAsync` method, call `ICommandHandler<T>`, and have no dependency on any job-runner library. The Infrastructure job runner (Hangfire, etc.) discovers them by type reference and calls `ExecuteAsync`.

`Infrastructure/BackgroundJobs/` is the **library client layer** — it owns adapters that implement `IBackgroundJob` and the configurator that registers recurring schedules. It has no business logic.

```
Application/Outbox/
└── OutboxMessageCleanupJob.cs           # job logic — calls ICommandHandler, no Hangfire dep

Application/AuditLogs/
└── DeleteOldAuditLogsBackgroundJob.cs   # job logic — calls ICommandHandler, no Hangfire dep

Infrastructure/BackgroundJobs/
├── HangfireBackgroundJob.cs             # IBackgroundJob adapter (wraps Hangfire client)
├── SimpleBackgroundJob.cs               # IBackgroundJob adapter (simple/test runner)
└── Hangfire/
    └── HangfireRecurringJobConfigurator.cs  # registers OutboxMessageCleanupJob, etc. in Hangfire
```

**Rule:** A class that ends with `BackgroundJob` but does NOT implement `IBackgroundJob` is job logic and belongs in Application. `Infrastructure/BackgroundJobs/` contains only library adapters (`IBackgroundJob` implementations) and the configurator that wires them to the job-runner SDK.

### Decision table

| What | Where | Why |
|------|-------|-----|
| DDD primitives (`Entity`, `Result`, `EncryptedString`, …) | `SharedKernel/` root | Inherited/implemented by all layers |
| Cross-slice value objects (`PhoneNumber`, `Money`, …) | `SharedKernel/<Concept>/` | Shared across slices without cross-slice coupling |
| Infrastructure interface contracts (`IEmailSender`, `IDateTimeProvider`, …) | `Application/Abstractions/` | Implemented by Infrastructure, consumed by Application |
| Options classes (`SmsOptions`, `EmailOptions`, …) | `Application/Abstractions/<Feature>/` | Shape defined by Application; Infrastructure binds to config source |
| Hosted services (`OutboxMessageHostedService`, …) | `Infrastructure/<Feature>/` | Infrastructure concern; registered in Infrastructure DI |
| Background job logic (`OutboxMessageCleanupJob`, `DeleteOldAuditLogsBackgroundJob`, …) | `Application/<Feature>/` | Calls command handlers; no job-runner library dependency |
| Background job library adapters (`HangfireBackgroundJob`, …) | `Infrastructure/BackgroundJobs/` | Implements `IBackgroundJob`; wraps Hangfire/other SDK |
| Slice-specific value objects | `Domain/<Feature>/` | Owned by one slice only |

## Feature Complexity Patterns

### Simple Features
- **Structure**: 1-2 operations, minimal logic
- **Examples**: Profiles, Tenants, AuditLogs
- **Pattern**:
  ```
  Domain/<Feature>/<Feature>.cs, <Feature>Errors.cs
  Application/<Feature>/<Operation1>/, <Operation2>/
  Infrastructure/<Feature>/<Feature>Configuration.cs
  Web.Api/Endpoints/<Feature>/<Operation1>.cs, <Operation2>.cs
  ```

### Medium Features
- **Structure**: 3-5 operations, standard CRUD + business logic
- **Examples**: Roles, Notifications, PushNotifications
- **Pattern**: Standard structure + Data folder + Events (optional)

### Complex Features
- **Structure**: 6+ operations, multiple sub-areas, nested organization
- **Examples**: Users, Authentication, Emails
- **Pattern**: Standard structure + nested subfolders for logical grouping
  ```
  Application/<Feature>/
    ├── <SubArea1>/
    │   ├── <Operation1>/
    │   └── <Operation2>/
    ├── <SubArea2>/
    │   └── <Operation3>/
    └── Data/
  ```
- **Real Examples**:
  - **Users**: AddRole/, RemoveRole/, EmailConfirmation/Confirm/, EmailConfirmation/ReSend/
  - **Authentication**: EmailCodeLogin/, EmailPassword/, PhoneCodeLogin/, Password/, RefreshTokens/
  - **Emails**: Builders/, Delivery/, Templates/, UserEmails/

## Class/Record/Interface Naming Conventions

| Element | Pattern | Example |
|---------|---------|---------|
| Domain Entity | `<Feature>.cs` | `User.cs`, `Role.cs` |
| Domain Errors | `<Feature>Errors.cs` | `UserErrors.cs`, `RoleErrors.cs` |
| Domain Event | `<Event>DomainEvent.cs` | `UserRegisteredDomainEvent.cs` |
| Command | `<Operation>Command.cs` | `CreateRoleCommand.cs` |
| Query | `<Operation>Query.cs` | `GetUserByIdQuery.cs` |
| Handler | `<Operation>CommandHandler.cs` | `CreateRoleCommandHandler.cs` |
| Response DTO | `<Operation>Response.cs` or `<Feature>Response.cs` | `UserResponse.cs` |
| CachedRepository | `<Feature>CachedRepository.cs` | `UserCachedRepository.cs` |
| Repository Interface | `I<Feature>CachedRepository.cs` | `IUserCachedRepository.cs` |
| EF Configuration | `<Entity>Configuration.cs` | `UserConfiguration.cs` |
| Seeder | `<Feature>Seeder.cs` | `UserSeeder.cs` |
| Endpoint | `<Operation>.cs` | `CreateRole.cs`, `GetUserById.cs` |
| Permissions | `<Feature>PermissionsConstants.cs` | `UserPermissionsConstants.cs` |

## Data Access Patterns

### Reads (Queries)
- **Location**: `Application/<Feature>/Data/`
- **Pattern**: CachedRepository classes with HybridCache
- **Returns**: Response DTOs (NEVER domain entities)
- **Interface**: `I<Feature>CachedRepository`
- **Implementation**: `<Feature>CachedRepository`

### Writes (Commands)
- **Location**: Command handlers use DbContext directly
- **Pattern**: Inject `IApplicationDbContext`
- **No caching**: Direct database writes

## Architecture Validation

### Layer Dependencies (Enforced by Architecture Tests)
```
Domain: NO dependencies (pure domain logic)
Application: Domain + SharedKernel ONLY
Infrastructure: Application + Domain + SharedKernel
Web.Api: ALL layers
```

### Feature Consistency Checklist

For each feature, verify:
- [ ] Domain entity exists in `Domain/<Feature>/`
- [ ] Domain errors exist in `Domain/<Feature>/<Feature>Errors.cs`
- [ ] Commands/Queries in `Application/<Feature>/`
- [ ] CachedRepository in `Application/<Feature>/Data/` (for reads)
- [ ] EF configuration in `Infrastructure/<Feature>/<Feature>Configuration.cs`
- [ ] Endpoints in `Web.Api/Endpoints/<Feature>/`
- [ ] Unit tests in `tests/Application.UnitTests/<Feature>/`
- [ ] Integration tests in `tests/Api.IntegrationTests/<Feature>/`
- [ ] Permissions constants defined (if secured)
- [ ] All handlers return `Result` or `Result<T>`
- [ ] All endpoints implement `IEndpoint`

## Cross-Feature Communication

Features communicate through well-defined boundaries:

| Method | Use Case | Example |
|--------|----------|---------|
| **Domain Events** | Async, decoupled notifications | `UserRegisteredDomainEvent` |
| **CachedRepository** | Read data from other features | Orders query Users via `IUserCachedRepository` |
| **Shared DTOs** | Public Response objects | `UserResponse`, `RoleResponse` |

**Anti-Patterns**:
- ❌ Direct handler-to-handler calls
- ❌ Feature A modifying Feature B's entities
- ❌ Shared domain entities across features

## Quick Reference

- **Architecture Overview**: See [Architecture.md](Architecture.md)
- **Adding New Features**: See [FeatureTemplates.md](FeatureTemplates.md)
- **Domain Events**: See [DomainEvent.md](DomainEvent.md)
- **Code Generator**: `dotnet run --project tools/CodeGenerator -- gen-entity -n <EntityName>`
