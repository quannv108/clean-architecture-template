Things that run *through* the [components](../components/index.md) rather than living in one of them. C4
has no level for these, and in this codebase they carry more of the design than the component diagram does.

# Structure

* [Layered Architecture](layered-architecture.md) - The six layers of the solution and the single direction in which dependencies may point.
* [Vertical Slice Architecture](vertical-slice-architecture.md) - Organising code by business capability across every layer, so one feature is one set of folders with the same name.

# The request path

* [Request Lifecycle](request-lifecycle.md) - What happens to an HTTP request from the route table to the committed transaction and the dispatched domain event.
* [API Surface](api-surface.md) - The versioned api/v1 route group, endpoint discovery, OpenAPI metadata, tags and ProblemDetails responses.
* [CQRS](cqrs.md) - Commands and queries as first-class types with dedicated handlers, resolved by Scrutor and injected directly - no mediator.
* [Decorator Pipeline](decorator-pipeline.md) - The four Scrutor decorators wrapped around every command and query handler, outermost to innermost.
* [Data Access](data-access.md) - Reads go through cached repositories returning DTOs; writes inject the DbContext directly. The two paths never cross.

# Asynchrony

* [Domain Event Dispatch](domain-event-dispatch.md) - How an event raised on an entity becomes an outbox row and later reaches its handlers - asynchronously, never in-memory.
* [Background Processing](background-processing.md) - Hosted services, recurring jobs and the split between job logic in Application and job-runner adapters in Infrastructure.

# State

* [Persistence](persistence.md) - PostgreSQL through EF Core - contexts, configurations, interceptors, converters, migrations, seeding and schema.
* [Caching Tiers](caching-tiers.md) - The HybridCache L1/L2 topology - in-memory per instance, optional Redis shared across instances, selected by configuration.

# Operations

* [Security and Authorization](security-and-authorization.md) - Identity through IUserContext, per-feature permission constants, rate-limit and CORS policies, and what the audit trail records.
* [Observability](observability.md) - Structured logging with Serilog into Seq, OpenTelemetry traces and metrics, and health checks.
