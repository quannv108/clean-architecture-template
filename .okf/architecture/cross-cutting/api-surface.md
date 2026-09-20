---
type: Mechanism
title: API Surface
description: The versioned api/v1 route group, endpoint discovery, OpenAPI metadata, tags and ProblemDetails responses.
resource: src/Web.Api/Extensions/EndpointExtensions.cs
tags: [http, api, versioning, openapi, routing]
status: stable
---

# API Surface

## The api/v1 group

[`EndpointExtensions.MapEndpoints`](../../../src/Web.Api/Extensions/EndpointExtensions.cs) discovers every
[`IEndpoint`](../../../src/Web.Api/Endpoints/IEndpoint.cs) in the assembly and registers it inside
`app.MapGroup("api/v1")`.

**The path written in an endpoint file is relative to that group.** `MapPost("/users", ...)` is reachable at
`/api/v1/users`. Calling the bare path from JavaScript, a test or curl returns 404. This is the single most
common mistake against this codebase.

Version `1.0` is the default, so no `api-version` parameter is required from clients.

## Discovery

Registration is by scan, not by an explicit list. An endpoint class that does not implement `IEndpoint`, or
is not in the `Web.Api` assembly, simply never appears - with no error. If a route 404s, check the interface
first.

## OpenAPI

Every endpoint must declare its full contract in the builder chain:

* `.WithName(nameof(<Operation>))`
* `.Accepts<TRequest>("application/json")` on POST and PUT
* `.Produces<T>(StatusCodes.Status200OK)` and `.ProducesProblem(...)` for each error status
* `.WithTags(Tags.<Feature>)` - the constant must exist in [`Tags`](../../../src/Web.Api/Endpoints/Tags.cs)
* `.AddOpenApiOperationTransformer` setting `Summary` and `Description`

Configured in `Extensions/OpenApi/`. The generated document is the contract clients build against, so an
undeclared response shape is a defect even when the code works.

## Errors

Failures are ProblemDetails responses produced by
[`CustomResults.Problem`](../../../src/Web.Api/Infrastructure/CustomResults.cs) from an
[`Error`](../../../src/SharedKernel/Error.cs)'s [`ErrorType`](../../../src/SharedKernel/Error.cs): Validation 400, NotFound 404,
Conflict 409, Problem 412. Anything unhandled is converted by
[`GlobalExceptionHandler`](../../../src/Web.Api/Infrastructure/GlobalExceptionHandler.cs) into a 500 ProblemDetails with no
internal detail leaked.

## Paging

List endpoints return [`PagedList<T>`](../../../src/Web.Api/Endpoints/Generic/PagedList.cs). Audit log queries page by cursor on
`ActionDateTime` rather than by offset.

## Operational endpoints

`GET /api/v1/version` and `GET /api/v1/environment` (`Endpoints/Information/`) answer the first incident
question - which build is deployed where. They return only the build identifier and environment name, never
configuration values.

## Dev pages

`Pages/Dev/` (`Email`, `Outbox`, `OutboxType`) show captured emails and pending/processed/failed outbox rows.
"The command succeeded but nothing happened" is almost always an undispatched or failed outbox message, and
these pages show which. **Development only** - they expose internal state.

## Related

* [Minimal API Endpoint](../../engineering/patterns/minimal-api-endpoint.md)
* [Route Conventions](../../engineering/conventions/route-conventions.md)
