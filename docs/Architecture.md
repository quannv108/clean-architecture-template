# Architecture

This is a .NET 10 Clean Architecture template implementing DDD with CQRS and Vertical Slice Architecture.

## Layer Dependencies (enforced by ArchitectureTests)

```
SharedKernel (no dependencies)
     ↑
Domain (only SharedKernel)
     ↑
Application (Domain + SharedKernel)
     ↑
Infrastructure (Application, Domain, SharedKernel)
     ↑
Web.Api (all layers)
```

## Project Structure

- **SharedKernel** - Common DDD abstractions: `Entity`, `AuditedEntity`, `ValueObject`, `Result<T>`, `Error`, `IDomainEvent`, `EncryptedString`
- **Domain** - Pure business logic, entities, domain events, value objects
- **Application** - Use cases, CQRS handlers, CachedRepositories, decorator pipeline
- **Infrastructure** - EF Core, PostgreSQL, authentication, external services, Outbox processing
- **Web.Api** - Minimal API endpoints using `IEndpoint` pattern
- **AppHost** - .NET Aspire orchestration
- **ServiceDefaults** - Aspire service defaults (health checks, telemetry)

## CQRS: Scrutor Decorators, NOT MediatR

This codebase does **not** use MediatR. Handlers are registered via Scrutor assembly scanning and injected directly into endpoints. There is no `IMediator.Send()`.

**Handler interfaces** (in `Application/Abstractions/Messaging/`):
- `ICommandHandler<TCommand>` → returns `Task<Result>`
- `ICommandHandler<TCommand, TResponse>` → returns `Task<Result<TResponse>>`
- `IQueryHandler<TQuery, TResponse>` → returns `Task<Result<TResponse>>`
- `IDomainEventHandler<TEvent>` → returns `Task`

**Decorator pipeline** (outermost → innermost, configured in `Application/DependencyInjection.cs`):
```
LoggingDecorator → ConcurrencyExceptionDecorator → ValidationDecorator → OpenTelemetryInstrumentDecorator → Handler
```
- **LoggingDecorator**: Logs start/completion/failure of all handlers
- **ConcurrencyExceptionDecorator**: Catches `DbUpdateConcurrencyException`, returns `ConcurrencyErrors.UpdateConflict()`
- **ValidationDecorator**: Runs `DataAnnotations.Validator` on commands before handler executes
- **OpenTelemetryInstrumentDecorator**: Creates Activity spans with operation metadata

**Endpoint handler injection** — endpoints inject handlers directly:
```csharp
app.MapPost("/users", async (
    CreateUserCommand command,
    ICommandHandler<CreateUserCommand, Guid> handler,
    CancellationToken ct) =>
{
    Result<Guid> result = await handler.Handle(command, ct);
    return result.Match(Results.Ok, CustomResults.Problem);
});
```

**Route prefix (`api/v1`)** — `MapEndpoints` (in `Web.Api/Extensions/EndpointExtensions.cs`) registers every `IEndpoint` inside a versioned group: `app.MapGroup("api/v1")`. The path passed to `MapPost`/`MapGet` is **relative to that group**, so the example above is reachable at `/api/v1/users`, not `/users`. The route declared in an endpoint file is never the full URL — always prepend `api/v1` when calling an endpoint from JS, tests, or external clients. The default version (`1.0`) is assumed when unspecified, so no `api-version` parameter is required.

## Data Access

- **Reads**: CachedRepository classes in `Application/<Feature>/Data/` using HybridCache, returning DTOs (never entities) — see [Caching.md](Caching.md)
- **Writes**: Direct `IApplicationDbContext` injection in command handlers — see [Concurrency.md](Concurrency.md)
- **Soft delete**: Global query filter on all `Entity` subclasses (`IsDeleted == false`) — applied automatically by EF Core
- **Optimistic concurrency**: PostgreSQL `xmin` column mapped to `Entity.Version` as row version — see [Concurrency.md](Concurrency.md)
- **Enums**: Stored as strings (convention in `BaseApplicationDbContext`)
- **Encrypted fields**: Use `EncryptedString` value object with `{KeyVersion}:{EncryptedValue}` format — see [Encryption.md](Encryption.md)

## Domain Events & Outbox Pattern

Domain events are dispatched **asynchronously via the Outbox pattern**, not in-memory: entities `Raise(...)` events, `SaveChangesAsync()` persists them as `OutboxMessage` records in the same transaction, and `OutboxMessageHostedService` polls and dispatches them to `IDomainEventHandler<T>` implementations.

See [DomainEvent.md](DomainEvent.md) for defining/handling events and [OutboxPattern.md](OutboxPattern.md) for the dispatch flow.

## Vertical Slice Organization

Features organized by business capability across all layers:
```
Domain/<Feature>/<Feature>.cs, <Feature>Errors.cs
Application/<Feature>/<Operation>CommandHandler.cs, Data/
Infrastructure/<Feature>/<Feature>Configuration.cs
Web.Api/Endpoints/<Feature>/<Operation>.cs
```

See [VerticalSliceStructure.md](VerticalSliceStructure.md) for the full slice layout and [FeatureTemplates.md](FeatureTemplates.md) for per-complexity templates.

## Code Conventions

### Record Syntax
- **Web.Api**: Positional syntax — `public record MyRecord(string Prop1, int Prop2);`
- **Application (Commands/Queries)**: Standard syntax with DataAnnotations for validation:
  ```csharp
  public sealed record CreateUserCommand : ICommand<Guid>
  {
      [Required]
      public string Email { get; init; }
  }
  ```

### Visibility
- Infrastructure services: `internal sealed`
- Handlers: `internal sealed`
- Endpoints: `internal sealed` implementing `IEndpoint`
- Domain errors: `public static` factory methods
- DbContext: `internal`

### Entity Creation
- Private constructors with public static `Create()` factory methods returning `Result<T>`
- Validation in factory methods
- IDs generated via `Guid.CreateVersion7()` (by `EntityIdGenerationInterceptor`)

### Result → HTTP Mapping
Endpoints use `Result.Match()` to convert to HTTP responses:
```csharp
result.Match(Results.Ok, CustomResults.Problem);
```
`CustomResults.Problem` maps `ErrorType` → HTTP status: Validation→400, NotFound→404, Conflict→409, Problem→412.

## Naming Conventions

See the full naming-convention table in [VerticalSliceStructure.md](VerticalSliceStructure.md#classrecordinterface-naming-conventions) (Domain entities/errors/events, commands, queries, handlers, DTOs, repositories, EF configs, seeders, endpoints, permissions).

## Error Codes

Domain error codes use the `"{Entity}.{ErrorName}"` pattern (e.g. `"User.NotFound"`, `"Order.InvalidStatusTransition"`). These go in the `Error.Code` field, defined as `public static` factory methods in `<Feature>Errors.cs`.
