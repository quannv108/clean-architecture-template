---
paths:
  - "src/Web.Api/Endpoints/**"
---

# Web.Api Endpoint Rules

Code template: [docs/FeatureTemplates.md → 4. Web.Api Layer](../../docs/FeatureTemplates.md#4-webapi-layer)

## Structure

- `internal sealed class <Operation> : IEndpoint` in `Endpoints/<Feature>/<Operation>.cs` — e.g. `CreateRole.cs`, **not** `CreateRoleEndpoint.cs`.
- Route handler is a method reference to `private static HandleAsync` — never an inline lambda in `MapEndpoint`.
- Request/response types: **positional records** defined in the same file.

## Required builder chain

- `.WithName(nameof(<Operation>))`
- `.Accepts<TRequest>("application/json")` on POST/PUT
- `.Produces<T>()` for success + `.ProducesProblem()` for each error status
- `.WithTags(Tags.<Feature>)` — add the constant to `Endpoints/Tags.cs` if new
- `.AddOpenApiOperationTransformer` setting `Summary` and `Description`

## Handling

- Inject `ICommandHandler<T,R>` / `IQueryHandler<T,R>` directly (no mediator).
- Map results with `result.Match(Results.Ok, CustomResults.Problem)` — `ErrorType` → HTTP: Validation→400, NotFound→404, Conflict→409, Problem→412.

## Routing

- Every endpoint is mounted under the `api/v1` group — the path in `MapPost`/`MapGet` is relative; the real URL is `/api/v1/...`. See [docs/Architecture.md → Route prefix](../../docs/Architecture.md#cqrs-scrutor-decorators-not-mediatr).
