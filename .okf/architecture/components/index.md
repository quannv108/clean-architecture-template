The five layer projects inside the [Web.Api container](../containers/web-api.md), plus the two that sit
outside the business stack. A component here is a **project**, not a class — classes is
the source itself - each component page links the files it owns.

# The business stack

Dependencies point inward only — [Layered Architecture](../cross-cutting/layered-architecture.md) has the
diagram and the reasoning; [Slices](../cross-cutting/vertical-slice-architecture.md) are the orthogonal axis.

* [SharedKernel Layer](shared-kernel.md) - The dependency-free project holding DDD primitives and cross-slice value objects that every other layer may use.
* [Domain Layer](domain.md) - Pure business logic - entities, domain events, domain errors and slice-local value objects, referencing only SharedKernel.
* [Application Layer](application.md) - Use cases - CQRS command and query handlers, the abstractions Infrastructure implements, options shapes, cached repositories and job logic.
* [Infrastructure Layer](infrastructure.md) - Implementations of Application abstractions - EF Core and PostgreSQL, cryptography, messaging adapters, lock providers, storage, hosted services.
* [Web.Api Layer](web-api.md) - The HTTP surface - minimal API endpoints implementing IEndpoint, middleware, result-to-response mapping and OpenAPI metadata.

# Outside the stack

* [ServiceDefaults](service-defaults.md) - Shared Aspire service wiring - health checks, OpenTelemetry, service discovery and HTTP resilience - applied by every hosted application.

[AppHost](../containers/apphost.md) is listed as a container rather than a component: it is a separate
process, not a part of the application.
