External frameworks, libraries and services, and what each is used for **here**. Check this list before
adding a dependency - the capability may already exist.

# Platform

* [.NET 10](dotnet-10.md) - The target framework, pinned by global.json.
* [.NET Aspire](dotnet-aspire.md) - The orchestration stack that runs the API together with its dependencies for local development.
* [Podman](podman.md) - The default container runtime for the local Aspire stack.

# Data

* [PostgreSQL](postgresql.md) - The database, and the source of two features the architecture depends on: xmin row versions and advisory locks.
* [Entity Framework Core](ef-core.md) - The ORM: contexts, configurations, interceptors, value converters, query filters and migrations.
* [Redis](redis.md) - Optional: enables the L2 cache tier and the Redis distributed lock provider when a connection string is configured.
* [HybridCache](hybrid-cache.md) - The two-tier caching API: in-memory L1 plus an optional distributed L2, behind one interface.

# Application

* [Scrutor](scrutor.md) - Assembly-scanning DI registration and decoration - how handlers are registered and the decorator pipeline is built.
* [Hangfire](hangfire.md) - The background job server behind the IBackgroundJob adapter and recurring schedules.
* [Medallion.Threading](medallion-threading.md) - The distributed locking library behind both lock providers.

# Observability

* [Serilog](serilog.md) - Structured logging, shipped to Seq.
* [Seq](seq.md) - The structured log server used locally, at http://localhost:8081.
* [OpenTelemetry](opentelemetry.md) - Traces and metrics, wired by ServiceDefaults.

# External services

* [Amazon SES](aws-ses.md) - The email provider behind SesEmailSender.
* [Twilio](twilio.md) - The SMS provider behind TwilioSmsSender.

# Testing

* [Testcontainers](testcontainers.md) - Real PostgreSQL in a container for integration tests.
* [NSubstitute](nsubstitute.md) - The mocking library - used instead of Moq, enforced by architecture tests.
* [Shouldly](shouldly.md) - The assertion library - used instead of FluentAssertions, enforced by architecture tests.
* [NetArchTest](netarchtest.md) - The library the architecture rules are written with.

# Template

* [Technology Template](_template-technology.md) - Copy this when adding an external framework, library or service.
