How the system is run, verified and shipped. Descriptive — the step-by-step commands are in
[Workflows](../../workflows/index.md).

# Concepts

* [Local Development Environment](local-development-environment.md) - Running the full stack locally with .NET Aspire, and the Podman/Docker container runtime choice.
* [Test Architecture](test-architecture.md) - Three test projects with three different jobs - architecture rules, Application unit tests, and end-to-end API integration tests.
* [CI Pipeline](ci-pipeline.md) - GitHub Actions builds the solution, runs every test including architecture tests, and publishes code coverage on each push and pull request.
