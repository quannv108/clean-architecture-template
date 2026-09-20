One file per term, tagged by subject. If a word in a document, a pull request or a conversation would make
a new reader pause, it should have an entry here.

Each entry defines the term generally, then says what it means **in this codebase** - which is the part a
general definition cannot give.

# Architecture

* [Aggregate Root](aggregate-root.md) - The entity that owns a cluster of objects and is the only entry point for changing them.
* [Bounded Context](bounded-context.md) - A boundary within which a domain term has one unambiguous meaning.
* [Clean Architecture](clean-architecture.md) - An architecture in which dependencies point inward toward the domain, so business rules depend on nothing.
* [Domain-Driven Design (DDD)](domain-driven-design.md) - Modelling software around the business domain, with behaviour on entities and a shared vocabulary between code and business.
* [Entity](entity.md) - An object with identity that persists across state changes.
* [Shared Kernel](shared-kernel.md) - The dependency-free set of types every layer may use.
* [Ubiquitous Language](ubiquitous-language.md) - The shared vocabulary of the business, used unchanged in code, tests and conversation.
* [Value Object](value-object.md) - An object defined by its values, with no identity.
* [Vertical Slice](vertical-slice.md) - A feature's code across every layer, grouped by business capability rather than technical role.

# Application

* [Assembly Scanning](assembly-scanning.md) - Registering or discovering types by reflecting over an assembly instead of listing them.
* [CQRS](cqrs.md) - Command Query Responsibility Segregation - separate types and paths for writes and reads.
* [Command](command.md) - A request to change state, as a record with a handler.
* [Composition Root](composition-root.md) - The single place where the object graph is wired up.
* [DTO](dto.md) - A Data Transfer Object - a flat shape for moving data across a boundary, with no behaviour.
* [Decorator](decorator.md) - A wrapper around a handler that adds cross-cutting behaviour without the handler knowing.
* [Dependency Injection](dependency-injection.md) - Supplying a class's collaborators from outside rather than constructing them internally.
* [Handler](handler.md) - The class that executes one use case - a command, a query or a domain event.
* [Query](query.md) - A request for data that changes nothing, as a record with a handler returning a DTO.
* [Repository](repository.md) - A class that mediates access to persisted data for one slice.
* [Response DTO](response-dto.md) - The read-side shape returned by queries and cached repositories.

# Asynchrony

* [At-Least-Once Delivery](at-least-once-delivery.md) - A guarantee that a message is delivered, possibly more than once.
* [Dead Letter](dead-letter.md) - A message parked after repeated failed delivery, so it stops consuming retry capacity.
* [Domain Event](domain-event.md) - A record of something significant that happened in the domain, dispatched to handlers asynchronously.
* [Eventual Consistency](eventual-consistency.md) - State that becomes consistent after a delay rather than immediately.
* [Fire-and-Forget](fire-and-forget.md) - Starting work without awaiting it or acting on its outcome.
* [Hosted Service](hosted-service.md) - A long-running background component started and stopped with the application host.
* [Idempotency](idempotency.md) - The property that performing an operation more than once has the same effect as performing it once.
* [Job Logic](job-logic.md) - The class that does the work of a background job, with no dependency on the job runner.
* [Leader Election](leader-election.md) - Choosing one instance to perform work that must happen only once.
* [Outbox](outbox.md) - A table of pending messages written inside the business transaction and dispatched afterwards.
* [Recurring Job](recurring-job.md) - Work executed on a schedule by a job server.

# Data

* [Advisory Lock](advisory-lock.md) - A PostgreSQL lock with application-defined meaning, not attached to any row or table.
* [GUID Version 7](guid-v7.md) - A time-ordered UUID, used for every entity identifier here.
* [Global Query Filter](global-query-filter.md) - An EF Core predicate applied automatically to every query for an entity type.
* [Interceptor](interceptor.md) - An EF Core hook that runs during operations such as SaveChanges.
* [Migration](migration.md) - A versioned, code-defined database schema change.
* [Model Snapshot](model-snapshot.md) - The generated file recording EF Core's current view of the model, used to diff the next migration.
* [Optimistic Concurrency](optimistic-concurrency.md) - Detecting a conflicting concurrent write at save time rather than preventing it with a lock.
* [Pessimistic Locking](pessimistic-locking.md) - Taking a lock before reading so nobody else can change the row until you commit.
* [Seeding](seeding.md) - Inserting baseline reference data at startup.
* [Soft Delete](soft-delete.md) - Marking a row deleted instead of removing it, and hiding it with a query filter.
* [Unit of Work](unit-of-work.md) - A boundary within which changes are tracked and committed together.
* [Value Converter](value-converter.md) - An EF Core translation between a property's CLR type and its stored representation.
* [xmin](xmin.md) - The PostgreSQL system column holding the transaction id that last wrote a row, used here as the EF row version.

# Caching

* [Cache Invalidation](cache-invalidation.md) - Removing a cached entry when the underlying data changes.
* [Cache Stampede](cache-stampede.md) - Many concurrent requests all missing the same key and hitting the database together.
* [L1 Cache](l1-cache.md) - The in-memory cache tier, local to one process.
* [L2 Cache](l2-cache.md) - The distributed cache tier, shared across instances, backed by Redis when configured.

# Security

* [4W Framework](four-w-framework.md) - Who, What, When, Where - the four facts every audit entry records.
* [Audit Trail](audit-trail.md) - A durable record of actions taken through the system.
* [ClaimsPrincipal](claims-principal.md) - The .NET representation of an authenticated caller and their claims.
* [Encryption at Rest](encryption-at-rest.md) - Encrypting stored data so the database file alone does not reveal it.
* [Hashing](hashing.md) - A one-way transformation used where a value must be checked but never read back.
* [Key Rotation](key-rotation.md) - Replacing an encryption key without losing access to data encrypted with the old one.
* [Key Version](key-version.md) - The identifier of the encryption key used for a value, stored with the ciphertext.
* [Multi-Tenancy](multi-tenancy.md) - Serving multiple customers from one deployment with their data kept separate.
* [Permission](permission.md) - A named capability a caller must hold to invoke an endpoint.

# Observability

* [Correlation ID](correlation-id.md) - An identifier shared by every log line and span belonging to one request.
* [Health Check](health-check.md) - An endpoint reporting whether the application and its dependencies are usable.
* [Span](span.md) - A timed unit of work within a distributed trace.
* [Structured Logging](structured-logging.md) - Emitting each log event as an object - a message template plus named properties - instead of a rendered string, so log tools can filter and aggregate by property.
* [Trace](trace.md) - The tree of spans describing one end-to-end operation.

# Testing

* [AAA Pattern](aaa-pattern.md) - Arrange, Act, Assert - the three-part structure of a unit test.
* [Architecture Test](architecture-test.md) - A unit test asserting a structural rule about the codebase.
* [Code Coverage](code-coverage.md) - The proportion of code executed by the test suite.
* [Test-Driven Development](tdd.md) - Writing or adjusting the test before the implementation.
* [Testcontainer](testcontainer.md) - A throwaway container started by a test run to provide a real dependency.

# HTTP

* [API Versioning](api-versioning.md) - Serving more than one version of an API contract.
* [CORS](cors.md) - Browser-enforced rules about which origins may call the API.
* [Cursor Pagination](cursor-pagination.md) - Paging by a position marker rather than a row offset.
* [Minimal API](minimal-api.md) - The ASP.NET Core endpoint model used here instead of MVC controllers.
* [Offset Pagination](offset-pagination.md) - Paging with skip and take.
* [OpenAPI](openapi.md) - The machine-readable description of the HTTP API, generated from endpoint metadata.
* [ProblemDetails](problem-details.md) - The RFC 7807 standard JSON shape for HTTP error responses.
* [Rate Limiting](rate-limiting.md) - Capping how often a caller may invoke an endpoint.
* [Route Group](route-group.md) - A prefix and shared configuration applied to a set of endpoints.

# Template

* [Term Template](_template-term.md) - Copy this when adding a glossary entry.
