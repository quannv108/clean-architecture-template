---
type: Component
title: Web.Api Layer
description: The HTTP surface - minimal API endpoints implementing IEndpoint, middleware, result-to-response mapping and OpenAPI metadata.
resource: src/Web.Api
tags: [layer, web-api, minimal-api, http]
status: stable
---

# Web.Api Layer

`src/Web.Api` references every layer below it. It owns transport: routes, status codes, serialization,
OpenAPI, middleware.

## What lives here

| Folder | Holds |
|---|---|
| `Endpoints/<Feature>/` | One file per operation, `internal sealed class <Operation> : IEndpoint` |
| `Endpoints/Tags.cs` | The OpenAPI tag constants every endpoint must use |
| `Extensions/` | `MapEndpoints`, CORS, rate limits, OpenAPI, Hangfire dashboard, audit log wiring |
| `Infrastructure/` | [`CustomResults`](../../../src/Web.Api/Infrastructure/CustomResults.cs), [`GlobalExceptionHandler`](../../../src/Web.Api/Infrastructure/GlobalExceptionHandler.cs), [`AuditAttribute`](../../../src/Web.Api/Infrastructure/AuditAttribute.cs) |
| `Middleware/` | [`AuditLoggingMiddleware`](../../../src/Web.Api/Middleware/AuditLoggingMiddleware.cs), [`RequestContextLoggingMiddleware`](../../../src/Web.Api/Middleware/RequestContextLoggingMiddleware.cs) |
| `Pages/Dev/` | [Razor dev pages](../../../src/Web.Api/Pages/Dev) for inspecting email and outbox state |

## The rules that define this layer

* **Every endpoint implements [`IEndpoint`](../../../src/Web.Api/Endpoints/IEndpoint.cs)** and is `internal sealed`. Discovery
  is by assembly scan in [`EndpointExtensions`](../../../src/Web.Api/Extensions/EndpointExtensions.cs); an endpoint that does
  not implement the interface silently never registers.
* **The route handler is a method reference to a `private static HandleAsync`,** never an inline lambda.
* **Every endpoint is mounted under the `api/v1` group.** The path in `MapPost`/`MapGet` is relative, so the
  real URL is `/api/v1/...`. See [API Surface](../cross-cutting/api-surface.md).
* **Handlers are injected directly** - `ICommandHandler<TCommand, TResponse>` as a parameter. No mediator.
* **`Result` is mapped, not unwrapped:** `result.Match(Results.Ok, CustomResults.Problem)`.
* **Request and response types are positional records in the same file** as the endpoint.
* **The builder chain is mandatory:** `.WithName`, `.Accepts<T>` on POST/PUT, `.Produces<T>()`,
  `.ProducesProblem()` per error status, `.WithTags(Tags.<Feature>)`, `.AddOpenApiOperationTransformer`.
  See [Minimal API Endpoint](../../engineering/patterns/minimal-api-endpoint.md).

## Result to HTTP mapping

[`CustomResults.Problem`](../../../src/Web.Api/Infrastructure/CustomResults.cs) maps [`ErrorType`](../../../src/SharedKernel/Error.cs):

| ErrorType | Status |
|---|---|
| Validation | 400 |
| NotFound | 404 |
| Conflict | 409 |
| Problem | 412 |

## Related

* [Minimal API Endpoint](../../engineering/patterns/minimal-api-endpoint.md) - the copyable code shape
* [Route Conventions](../../engineering/conventions/route-conventions.md)
