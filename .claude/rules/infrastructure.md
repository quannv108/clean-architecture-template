---
paths:
  - "src/Infrastructure/**"
---

# Infrastructure Layer Rules

Detail: [Infrastructure Layer](../../.okf/architecture/components/infrastructure.md),
[Persistence](../../.okf/architecture/cross-cutting/persistence.md)

## Visibility

- Everything is `internal sealed` except interfaces, extension classes, configuration types, constants and
  enums. Enforced: [Visibility](../../.okf/engineering/conventions/visibility.md).
- EF migrations and their `.Designer.cs` must be changed from `public partial` to **`internal partial`**
  after every `dotnet ef migrations add` —
  [Add an EF Core Migration](../../.okf/workflows/engineering/add-ef-migration.md).

## What belongs here

- Implementations of interfaces declared in `Application/Abstractions/` — adapters, nothing more. A business
  rule written here belongs in Domain or Application.
- Hosted services (`IHostedService` / `BackgroundService`), registered in `Infrastructure/DependencyInjection.cs`
  — never in Web.Api. Enforced:
  [Background Processing](../../.okf/architecture/cross-cutting/background-processing.md).
- Job-runner **adapters** implementing `IBackgroundJob`. A `*BackgroundJob` class that does *not* implement
  it is job logic and belongs in `Application/<Feature>/` —
  [Background Processing](../../.okf/architecture/cross-cutting/background-processing.md).

## Configuration

- Options classes are **defined in Application**; Infrastructure only binds them:
  `services.AddOptions<T>().BindConfiguration("X").ValidateOnStart()`.
- Never inject `IConfiguration` outside that binding call —
  [Options Pattern](../../.okf/engineering/patterns/options-pattern.md).

## Third-party SDKs

- The SDK type never leaves this layer. Wrap it in an adapter behind the Application abstraction —
  [Naming and Placement](../../.okf/engineering/conventions/naming.md).
- Package versions are pinned centrally in `Directory.Packages.props`, never in a `.csproj`.
