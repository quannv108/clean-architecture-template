# Clean Architecture Template

> **📘 For AI Assistants (Google Antigravity, Cursor, Claude, Qwen, Gemini, etc...)**: Always reference *
*[`agents.md`](agents.md)** before working on this codebase. It contains essential architecture patterns, naming
> conventions, layer dependencies, and development standards.

What's included in the template?

- SharedKernel project with common Domain-Driven Design abstractions.
- Domain layer with sample entities.
- Application layer with abstractions for:
    - CQRS
    - Example use cases
    - Cross-cutting concerns (logging, validation)
- Infrastructure layer with:
    - Authentication
    - Permission authorization
    - EF Core, PostgreSQL
    - Serilog
- Seq for searching and analyzing structured logs
    - Seq is available at http://localhost:8081 by default
- Testing projects
    - **Comprehensive Architecture Testing** with NetArchTest.Rules
        - **Layer Dependency Tests**: Ensures proper Clean Architecture layer dependencies
        - **Application Layer Tests**: Validates CQRS patterns, Result types, and vertical slice organization
        - **Domain Layer Tests**: Enforces domain entity inheritance, pure domain logic, and domain event patterns
        - **Infrastructure Layer Tests**: Ensures proper encapsulation and dependency injection patterns
        - **Presentation Layer Tests**: Validates endpoint patterns and internal visibility
        - **Code Quality Tests**: Enforces async naming conventions and documentation standards
        - **Testing Standards Tests**: Validates proper testing patterns and framework usage
    - **Unit tests** with high coverage requirements (70%+ for Application layer)
    - **Integration tests** for API endpoints
        - **CRITICAL RULE**: Integration tests MUST use API endpoints only
        - NEVER write directly to the database in integration tests
        - Always use HTTP calls to set up test data (e.g., `POST /users/register`, `POST /roles`)
        - Tests should verify the full request/response cycle, not bypass the application layer
- Continuous Integration with GitHub Actions
    - Automated build and test on push/PR
    - Code coverage reporting with coverlet and ReportGenerator
    - Coverage reports published as artifacts and GitHub Pages

# Migrations

* To create migrations, run the following command from root of the solution:

```bash
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/Web.Api -o Database/Migrations  --context ApplicationDbContext
```

* To apply migrations, run project Web.Api

# Continuous Integration

The project includes GitHub Actions workflows for automated build, test, and coverage reporting:

## CI Workflow Features

- **Build**: Automated build on every push and pull request
- **Test**: Runs all unit tests including comprehensive architecture tests
- **Coverage**: Generates code coverage reports using coverlet (free, open-source)
- **Reporting**:
    - Uploads coverage reports as artifacts
    - Displays coverage summary in PR comments
    - Publishes coverage reports to GitHub Pages (on main branch)

## Viewing Coverage Reports

- **Artifacts**: Download coverage reports from the Actions tab
- **GitHub Pages**: Visit `https://quannv108.github.io/clean-architecture/` for the latest coverage report (main branch)
- **PR Summary**: Coverage summary is automatically added to pull request checks

## Local Coverage

To generate coverage reports locally:

### Option 1: Use the provided scripts

```bash
# Linux/macOS
./scripts/ci-local.sh

# Windows
scripts\ci-local.bat
```

### Option 2: Manual commands

```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Install ReportGenerator (if not already installed)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML coverage report
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage/report" -reporttypes:"Html"
```

# Infrastructure Layer Responsibilities

The Infrastructure layer is responsible for all external concerns and technical implementations, including:

- Hashing (e.g., email, password)
- Encryption/decryption
- Password hashing and verification
- Database access through DbContext and CachedRepository pattern implementations
- Authentication and authorization
- Logging and monitoring
- Integration with external services (APIs, message queues, etc.)

> **Note:** Do not implement or inject infrastructure logic (such as hashing, encryption, or password hashing, caching)
> into the Domain layer. These concerns should be handled in the Application and Infrastructure layers, following Clean
> Architecture and DDD best practices.

## Data Access Pattern

This project uses a **CachedRepository pattern** instead of traditional repositories:

- **CachedRepository classes** (e.g., `UserCachedRepository`, `TodoCachedRepository`) handle read operations and are
  located in `.Data` namespaces within feature slices
- **Direct DbContext usage** in command handlers for write operations
- **HybridCache integration** in CachedRepository classes for performance optimization
- **Response DTOs** returned from CachedRepository classes instead of Domain entities

This approach provides:

- Clear separation between read and write operations (CQRS)
- Optimized queries with caching capabilities
- Better performance through targeted data retrieval
- Simplified testing without complex mocking

## Expand project

See [Feature Templates](docs/FeatureTemplates.md) with structure templates for Simple, Medium, and Complex features. For
analysis of existing features, see [Vertical Slice Structure](docs/VerticalSliceStructure.md).
