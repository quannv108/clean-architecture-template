---
paths:
  - "src/Web.Api/**"
---

# Web.Api Endpoint Rules

Code template: [.okf/engineering/patterns/minimal-api-endpoint.md](../../.okf/engineering/patterns/minimal-api-endpoint.md).
Enforced by: `tests/ArchitectureTests/Presentation/PresentationTests.cs` - see [.okf/engineering/constraints.md](../../.okf/engineering/constraints.md).

## Structure

- `internal sealed class <Operation> : IEndpoint` in `Endpoints/<Feature>/<Operation>.cs` — e.g.
  `CreateRole.cs`, **not** `CreateRoleEndpoint.cs`.
- A class that does not implement `IEndpoint` silently never registers — discovery is by assembly scan.
- Route handler is a method reference to `private static HandleAsync` — never an inline lambda.
- Request/response types: **positional records** defined in the same file.

## Required builder chain

- `.WithName(nameof(<Operation>))`
- `.Accepts<TRequest>("application/json")` on POST/PUT
- `.Produces<T>()` for success + `.ProducesProblem()` for each error status
- `.WithTags(Tags.<Feature>)` — add the constant to `Endpoints/Tags.cs` if new
- `.AddOpenApiOperationTransformer` setting `Summary` and `Description`
- `.WithAuditLog("ActionName")` on security-relevant endpoints —
  [.okf/engineering/patterns/audit-logging.md](../../.okf/engineering/patterns/audit-logging.md)

## Handling

- Inject `ICommandHandler<T,R>` / `IQueryHandler<T,R>` directly (no mediator).
- Map results with `result.Match(Results.Ok, CustomResults.Problem)` — `ErrorType` → HTTP: Validation→400,
  NotFound→404, Conflict→409, Problem→412.

## Routing

- Every endpoint is mounted under the `api/v1` group — the path in `MapPost`/`MapGet` is relative; the real
  URL is `/api/v1/...`. See
  [.okf/engineering/conventions/route-conventions.md](../../.okf/engineering/conventions/route-conventions.md).
