---
type: Pattern
title: "Minimal API Endpoint"
description: "The required shape of an endpoint: IEndpoint, a static HandleAsync, positional request records and a complete builder chain."
resource: src/Web.Api/Endpoints
tags: [web-api, endpoints, minimal-api, openapi]
status: stable
---

# Minimal API Endpoint

One class per operation, discovered by assembly scan and mounted under `api/v1`. The endpoint does three
things and nothing else: declare the route and its OpenAPI contract, map the request to a command or query,
and map the `Result` to a response.

## How

* `internal sealed class <Operation> : IEndpoint` in `Endpoints/<Feature>/<Operation>.cs` - named after the
  operation (`CreateRole.cs`), never `CreateRoleEndpoint.cs`.
* `MapEndpoint` registers the route with a **method reference** to `private static HandleAsync`, not a
  lambda - the chain stays readable and the handler is unit-testable.
* `HandleAsync` takes the request (body, route values), the handler interface and a `CancellationToken`;
  it builds the command or query, calls `Handle`, and returns `result.Match(Results.Ok, CustomResults.Problem)`.
  No hand-written status codes: the `ErrorType` decides them.
* Request and response types are positional records in the same file.
* The builder chain **is** the OpenAPI contract - every response shape, including each error status, is
  declared. An undeclared one is a defect even when the code works.

```csharp
app.MapPost("/features", HandleAsync)
    .WithName(nameof(CreateFeature))
    .Accepts<CreateFeatureRequest>("application/json")
    .Produces<Guid>()
    .ProducesProblem(StatusCodes.Status400BadRequest)   // one per error status the handler can return
    .WithTags(Tags.Features)
    .AddOpenApiOperationTransformer(...);              // Summary and Description
```

Real instances: [`SendTestEmail.cs`](../../../src/Web.Api/Endpoints/Emails/SendTestEmail.cs) (POST) and
[`GetAuditLogs.cs`](../../../src/Web.Api/Endpoints/AuditLogs/GetAuditLogs.cs) (GET with paging).

## Checklist

- [ ] `internal sealed class <Operation> : IEndpoint`, file named after the operation
- [ ] Route handler is a method reference to `private static HandleAsync`
- [ ] Request/response records in the same file
- [ ] `.WithName(nameof(<Operation>))`
- [ ] `.Accepts<TRequest>("application/json")` on POST/PUT
- [ ] `.Produces<T>()` plus `.ProducesProblem()` for every error status
- [ ] `.WithTags(Tags.<Feature>)` - add the constant to [`Tags`](../../../src/Web.Api/Endpoints/Tags.cs) if new
- [ ] `.AddOpenApiOperationTransformer` setting `Summary` and `Description`
- [ ] `result.Match(Results.Ok, CustomResults.Problem)`
- [ ] `.WithAuditLog("...")` if the endpoint is security-relevant - [Audit Logging](audit-logging.md)

## Two things that produce 404s

1. **The class does not implement `IEndpoint`.** Discovery is by scan, so it silently never registers.
2. **You called the bare path.** `/features` is really `/api/v1/features` -
   [API Surface](../../architecture/cross-cutting/api-surface.md).

Enforced by `tests/ArchitectureTests/Presentation/PresentationTests.cs` for the structural parts; the builder
chain is review.
