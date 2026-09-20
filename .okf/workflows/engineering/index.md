Procedures an engineer or an agent runs against the repository, with the exact commands.

# Building things

* [Add a Feature](add-a-feature.md) - The end-to-end procedure for adding a vertical slice, scaled to Simple, Medium or Complex.
* [Add an EF Core Migration](add-ef-migration.md) - The migration command with its two mandatory flags, and the visibility change that follows every time.

# Running and verifying

* [Build and Test](build-and-test.md) - The command set for building, testing and running the solution.
* [Run the Stack Locally](run-the-stack.md) - Starting Postgres, pgweb, Seq and the API together with Aspire.
* [Run Integration Tests](run-integration-tests.md) - Running the Testcontainers-backed integration tests, including the two environment variables Podman needs.
* [Format Code](format-code.md) - Checking and fixing style violations without touching unrelated files.
* [Generate a Coverage Report](generate-coverage.md) - Producing the HTML coverage report locally.

---

Descriptive counterparts — what the test projects *are*, what CI *does* — live in
[Delivery](../../architecture/delivery/index.md).
