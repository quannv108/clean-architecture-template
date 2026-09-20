Why the codebase is the way it is. When a rule elsewhere in this bundle seems arbitrary, or you are about
to do something a different way, the reasoning is here.

**These records were reconstructed on 2026-09-19** from the codebase and its previous `docs/` tree. The
decisions were already in force; the context and rejected alternatives are inferred rather than recorded at
the time. Correct any you have first-hand knowledge of, and add
`verified: { by: human:<id>, at: <ISO8601> }` when you do.

# Concepts

* [ADR 0001: Clean Architecture with Vertical Slices](0001-clean-architecture-with-vertical-slices.md) - Combine inward-pointing layer dependencies with per-capability slices, and enforce the layering with tests rather than convention.
* [ADR 0002: No MediatR - handlers injected directly](0002-no-mediatr.md) - Register handlers with Scrutor and inject them into endpoints instead of routing calls through a mediator.
* [ADR 0003: Dispatch domain events asynchronously via the Outbox](0003-async-domain-events-via-outbox.md) - Persist raised domain events in the business transaction and dispatch them from a background service, rather than in-memory during SaveChanges.
* [ADR 0004: Optimistic concurrency with PostgreSQL xmin](0004-xmin-optimistic-concurrency.md) - Use the built-in xmin system column as the EF row version instead of a maintained version column or pessimistic locks.
* [ADR 0005: Two-tier caching with HybridCache](0005-hybridcache-two-tier.md) - Use HybridCache so the same code path serves an L1-only development setup and an L1+L2 Redis deployment.
* [ADR 0006: Versioned AES-256 field encryption via EncryptedString](0006-versioned-field-encryption.md) - Encrypt sensitive columns through a value type whose stored format carries its key version, so keys can rotate without a data migration.
* [ADR 0007: PostgreSQL advisory locks by default, Redis when configured](0007-lock-provider-selection.md) - Provide distributed locking through one abstraction with a provider chosen by the same setting that enables the L2 cache.
* [ADR 0008: Result<T> for expected failures, exceptions for the unexpected](0008-result-pattern-over-exceptions.md) - Return failures as values so they appear in signatures and map to HTTP statuses, keeping exceptions for genuinely exceptional conditions.
* [ADR 0009: Soft delete with a global query filter](0009-soft-delete-by-default.md) - Mark rows deleted rather than removing them, and hide them with a model-wide query filter.
* [ADR 0010: Minimal APIs with a discovered IEndpoint interface](0010-minimal-api-iendpoint.md) - Use minimal APIs, one class per operation implementing IEndpoint, discovered by assembly scan under a versioned group.
* [ADR 0011: Podman as the default container runtime](0011-podman-default-container-runtime.md) - Default Aspire to Podman in code so no per-developer setup is needed, with Docker available by environment variable.
* [ADR 0012: Infrastructure and endpoints are internal, enforced by tests](0012-internal-visibility-enforced.md) - Keep implementation types internal so the layer boundary is enforced by the compiler rather than by discipline.
* [ADR 0013: DataAnnotations for shape validation, domain errors for business rules](0013-dataannotations-validation.md) - Validate command shape with DataAnnotations in a decorator, and leave rules that need data to the domain.
* [ADR 0014: NSubstitute, Shouldly and Testcontainers as the test stack](0014-nsubstitute-shouldly-testcontainers.md) - Standardise on one mocking library, one assertion library and real containerised dependencies, enforced by architecture tests.
* [ADR 0015: Replace docs/ with an OKF knowledge base at .okf/](0015-adopt-okf-knowledge-base.md) - Decompose prose documentation into one concept per file in Open Knowledge Format, and reduce the agent entry point to a pointer.
* [ADR Template](_template-adr.md) - Copy this when recording an architecture decision.
