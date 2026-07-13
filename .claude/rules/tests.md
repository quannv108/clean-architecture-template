---
paths:
  - "tests/**"
---

# Testing Rules

Detail: [docs/DevelopmentGuideline.md → Testing Requirements](../../docs/DevelopmentGuideline.md#testing-requirements)

## Libraries & style

- AAA pattern (Arrange, Act, Assert).
- **NSubstitute** for mocking (not Moq); **Shouldly** for assertions (not FluentAssertions).
- DbSet mocking: `BuildMock()` from MockQueryable.NSubstitute.
- Unit test naming: `<Operation>HandlerTests.cs` in `tests/Application.UnitTests/<Feature>/`.
- Coverage target: 70%+ for the Application layer.

## Integration tests

- Go through **API endpoints only — never write to the database directly**.
- Use `ApiTestFactory` (WebApplicationFactory + Testcontainers PostgreSQL) and the `ApiClient` helper for authenticated HTTP calls.
- Endpoint URLs include the `/api/v1` prefix — the bare path from the endpoint file 404s.
- Call `WaitForOutboxMessagesAsync()` when asserting domain-event side effects (Outbox dispatch is async).
- Running under Podman needs `DOCKER_HOST` + `TESTCONTAINERS_RYUK_DISABLED` — see [docs/DevelopmentGuideline.md → Integration tests with Podman](../../docs/DevelopmentGuideline.md#integration-tests-with-podman-testcontainers).

## Before completing any work

- Run `dotnet test tests/ArchitectureTests/` — layer dependencies and visibility rules are enforced there.
