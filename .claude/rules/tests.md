---
paths:
  - "tests/**"
---

# Testing Rules

Detail: [.okf/architecture/delivery/test-architecture.md](../../.okf/architecture/delivery/test-architecture.md),
[.okf/workflows/engineering/run-integration-tests.md](../../.okf/workflows/engineering/run-integration-tests.md)

## Libraries & style

- AAA pattern (Arrange, Act, Assert).
- **NSubstitute** for mocking (not Moq); **Shouldly** for assertions (not FluentAssertions) — enforced by
  `tests/ArchitectureTests/Testing/TestingStandardsTests.cs`.
- DbSet mocking: `BuildMock()` from MockQueryable.NSubstitute.
- Unit test naming: `<Operation>HandlerTests.cs` in `tests/Application.UnitTests/<Feature>/`.
- Assert on `Error.Code`, not just that the result failed.
- Coverage target: 70%+ for the Application layer.

## Integration tests

- Go through **API endpoints only — never write to the database directly**.
  See [.okf/architecture/delivery/test-architecture.md](../../.okf/architecture/delivery/test-architecture.md).
- Use `ApiTestFactory` (WebApplicationFactory + Testcontainers PostgreSQL) and the `ApiClient` helper.
- Endpoint URLs include the `/api/v1` prefix — the bare path from the endpoint file 404s.
- Call `WaitForOutboxMessagesAsync()` when asserting domain-event side effects (dispatch is async).
- Running under Podman needs `DOCKER_HOST` + `TESTCONTAINERS_RYUK_DISABLED` — see
  [.okf/workflows/engineering/run-integration-tests.md](../../.okf/workflows/engineering/run-integration-tests.md).

## Before completing any work

- Run `dotnet test tests/ArchitectureTests/`. A failure there is a design error, not a test to loosen —
  [.okf/engineering/constraints.md](../../.okf/engineering/constraints.md).
