# AI Agents Configuration for Clean Architecture Template

## Overview

This document provides configuration and instructions for AI agents (Gemini, Qwen, Claude, and Kilocode) to work
effectively with the Clean Architecture Template. The template implements Domain-Driven Design (DDD) patterns with CQRS
and Vertical Slice Architecture in .NET 10.

## Project Overview

.NET 10 Clean Architecture template implementing Domain-Driven Design (DDD) with CQRS and Vertical Slice Architecture.
Built with EF Core, PostgreSQL, JWT authentication, permission-based authorization, and comprehensive architecture
testing.

## Essential Commands

### Build and Test

```bash
# Build solution
dotnet build CleanArchitecture.slnx

# Run all tests
dotnet test CleanArchitecture.slnx
```

## Core Project Information

### Repository Purpose

- **Clean Architecture Template** implementing DDD principles with CQRS and Vertical Slice Architecture
- **Technology Stack**: .NET, EF Core, PostgreSQL, JWT Authentication, Serilog, OpenTelemetry
- **Architecture**: SharedKernel, Domain, Application, Infrastructure, Web.Api layers
- **Testing**: Unit tests (70%+ coverage), Integration tests, Architecture compliance tests

### Key Architectural Patterns

1. **Clean Architecture**: Strict layer separation with dependency inversion
2. **Domain-Driven Design**: Rich domain models, domain events, value objects
3. **CQRS**: Commands (ICommand/ICommand<T>), Queries (IQuery<T>), Handlers (ICommandHandler/IQueryHandler)
4. **Vertical Slice Architecture**: Features organized by business capability
5. **Result Pattern**: Error handling via `Result<T>` and `Result.Failure()`

### Feature Set

- User management with JWT authentication and role-based permissions
- Multi-tenancy support with tenant isolation
- Communication systems (email, SMS, push notifications)
- Audit logging (4W compliance: Who, What, When, Where)
- Distributed locking (PostgreSQL default, Redis option)
- Background jobs with Hangfire
- Encrypted data storage with key rotation
- GDPR compliance features

## AI Agent Instructions

### General Guidelines for All Agents

1. **Understand the Architecture**: Always consider the layered architecture and dependency rules
    - Domain layer: No dependencies on other layers
    - Application layer: Only Domain and SharedKernel dependencies allowed
    - Infrastructure layer: Can depend on Application, Domain, SharedKernel
    - Web.Api layer: Can depend on all layers

2. **Respect CQRS Patterns**:
    - Commands implement `ICommand`/`ICommand<T>`
    - Queries implement `IQuery<T>`
    - Handlers implement `ICommandHandler<T>`/`IQueryHandler<T,R>`
    - Return `Task<Result>` or `Task<Result<T>>`

3. **Follow Naming Conventions**:
    - Feature-specific folder structure: `<feature>/<subfeature>/<verb>/file.cs`
    - Example: `Users/Authentication/Commands/LoginUserCommand.cs`
    - Never name folders as generic "Commands" or "Queries", use functional names

4. **Respect Testing Standards**:
    - Unit tests: 70%+ coverage for Application layer
    - Integration tests: Use API endpoints only to set up data, never direct DB access
    - Use Shouldly/NSubstitute (not FluentAssertions/Moq)

5. **Record Syntax Guidelines**:
    - **Web.Api Layer**: Use Positional Syntax for records (e.g.,
      `public record MyRecord(string Property1, int Property2);`)
    - **Application Layer (Queries and Commands)**: Use Standard Syntax with Validation Attributes to able to do
      command/query validation (e.g.,
      `public record MyQuery { [Required] public string Property1 { get; init; } [Range(1, 100)] public int Property2 { get; init; } }`)

### Architecture Constraints

1. **Layer Dependencies**: Follow strict dependency rules (enforced by ArchitectureTests)
2. **Domain Layer**: No infrastructure dependencies, pure business logic only
3. **Application Layer**: No direct infrastructure dependencies in handlers
4. **CachedRepository**: Only use HybridCache in `.Data` namespaces, return DTOs not entities
5. **Configuration**: Always use Options pattern with `IOptions<T>`, never `IConfiguration`

### Key Technologies to Consider

When providing suggestions or generating code:

- Use .NET latest features and patterns
- Implement proper async/await patterns (methods with 'Async' suffix except Handle/MapEndpoint)
- Use file-scoped namespaces
- Leverage Aspire for cloud-native development
- Consider OpenTelemetry integration for observability
- Apply Serilog for structured logging

### Common Development Tasks

1. **Adding New Features**: Use code generator or follow feature templates:
   ```bash
   dotnet run --project tools/CodeGenerator -- gen-entity -n <EntityName>
   ```

2. **Database Migrations**:
   ```bash
   dotnet ef migrations add <MigrationName> --project src/Infrastructure --startup-project src/Web.Api
   ```

3. **Build and Test**:
   ```bash
   dotnet build CleanArchitecture.slnx  # Build solution
   dotnet test CleanArchitecture.slnx   # Run all tests
   ```

4. **Local Development with Aspire**:
   ```bash
   dotnet run --project src/AppHost  # Run full stack with containers
   ```

### AI-Specific Instructions

- When working with this codebase, reference the centralized instructions in this document for general guidance
- Follow the established architecture patterns: Clean Architecture, DDD, CQRS, and Vertical Slice Architecture
- Use the proper layer dependencies: Domain -> Application -> Infrastructure -> Web.Api
- For detailed architectural guidance, see [docs/Architecture.md](docs/Architecture.md)

### Testing Considerations

- Unit tests should mock external dependencies using NSubstitute
- Integration tests must use WebApplicationFactory with Testcontainers
- Follow AAA pattern (Arrange, Act, Assert)
- Use `BuildMock()` from MockQueryable.NSubstitute for database mock sets
- Never write directly to database in integration tests

### Security & Performance

- Implement proper authentication/authorization checks
- Use distributed locks for cross-instance coordination
- Apply caching strategies (HybridCache with L1/L2)
- Consider optimistic concurrency with PostgreSQL `xmin`
- Encrypt sensitive data with AES-256

## Knowledge Sources

AI agents should reference these files for detailed project information:

### Core Documentation

- **[agents.md](agents.md)**: Centralized instructions for all AI agents (primary reference)
- [docs/Architecture.md](docs/Architecture.md) - Detailed architecture overview
- [docs/FeatureTemplates.md](docs/FeatureTemplates.md) - Feature implementation templates
- [docs/VerticalSliceStructure.md](docs/VerticalSliceStructure.md) - Structural organization
- [docs/DomainEvent.md](docs/DomainEvent.md) - Domain events implementation
- Memory bank: `.kilocode/rules/memory-bank/` contains structured knowledge

## Best Practices

1. **Always Verify**: Validate code against existing patterns before suggesting changes
2. **Be Specific**: Provide concrete examples based on the current codebase
3. **Respect Constraints**: Adhere to all architectural constraints and layer dependencies
4. **Consider Testing**: Include testing implications when suggesting changes
5. **Maintain Quality**: Ensure suggestions maintain the high code quality standards
6. **Update Documentation**: Suggest updates to relevant documentation when implementing changes

## Troubleshooting

When encounter issues:

1. **Build Failures**: Check for proper layer dependencies and missing using statements
2. **Test Failures**: Verify that domain logic is properly encapsulated and dependencies are mocked
3. **Architecture Test Failures**: Ensure no layer dependency rules are violated
4. **Performance Issues**: Consider caching strategies and avoid N+1 queries
5. **Security Issues**: Ensure proper authentication/authorization implementation