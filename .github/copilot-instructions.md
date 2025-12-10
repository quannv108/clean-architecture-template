# GitHub Copilot Instructions for Clean Architecture Project

## Project Overview
This is a .NET 9 Clean Architecture template implementing Domain-Driven Design (DDD) patterns with CQRS and **Vertical Slice Architecture**, following best practices for enterprise applications.

## Architecture Layers
- **SharedKernel**: Common DDD abstractions and base classes
- **Domain**: Core business logic, entities, domain events, and business rules
- **Application**: Use cases, commands/queries (CQRS), handlers, and application services
- **Infrastructure**: External concerns (database, authentication, logging, external APIs)
- **Web.Api**: API controllers and presentation layer

## Vertical Slice Architecture
- **MANDATORY**: Organize features by business capability, not technical layers
- Each feature should contain its complete vertical slice: Command/Query, Handler, Validator, Response DTOs
- Group related functionality together (e.g., all user-related operations)
- Minimize coupling between different feature slices
- Each slice should be independently testable and deployable

## Key Patterns and Practices

### Domain-Driven Design
- Use rich domain models with encapsulated business logic
- Implement domain events for cross-cutting concerns
- Follow aggregate root patterns
- Keep domain layer pure with no external dependencies

### CQRS Implementation
- Commands for write operations (mutations)
- Queries for read operations
- Separate handlers for commands and queries

### Error Handling
- Use Result pattern for error handling
- Domain-specific error types in each aggregate
- Avoid throwing exceptions for business rule violations

### Authentication & Authorization
- JWT-based authentication
- Permission-based authorization
- User and Profile entities with proper relationships

## Code Standards

### Build and Testing Requirements
- **MANDATORY**: Always build the solution after any change: `dotnet build CleanArchitecture.slnx`. If `slnx` is not available, use `sln` instead. If it failed to build, fix before running tests.
- **MANDATORY**: Run all tests after any change: `dotnet test CleanArchitecture.slnx`. If `slnx` is not available, use `sln` instead.
- **MANDATORY**: Maintain 100% build success and pass all tests
- **MANDATORY**: Every new feature MUST have comprehensive unit tests with high coverage
- **MANDATORY**: Write unit tests FIRST or alongside implementation, never after

### Unit Testing Standards
- **MANDATORY**: Minimum 70% code coverage for Application layer
- **MANDATORY**: Test all command/query handlers thoroughly
- **MANDATORY**: Test domain logic and application logic rules
- **MANDATORY**: Use Shouldly or xUnit Assert for assertions (do not use FluentAssertions)
- **MANDATORY**: When assertions with Received, specify the number of times a method should be called
- **MANDATORY**: Use NSubstitute for mocking (do not use Moq). When mocking a function response, use `Returns` method. If return type is Task, use `Returns(value)` and do not need to specify Task.FromResult.
- **MANDATORY**: Never use EntityFramework InMemory for unit tests
- **MANDATORY**: Mock external dependencies and infrastructure concerns. Don't mock static methods or properties.
- **MANDATORY**: Test both success and failure scenarios
- **MANDATORY**: Validate error handling and edge cases

### Code Quality and Formatting
- Nullable reference types enabled
- Treat warnings as errors
- Follow SonarAnalyzer rules
- Use implicit usings
- Enable latest analysis level
- **CRITICAL**: Never change existing spacing or braces
- **CRITICAL**: Never add new lines to end of files
- Follow existing indentation patterns
- Use consistent naming conventions
- Always use braces for control statements

### Entity Framework
- Use EF Core with PostgreSQL
- Migrations located in `src/Infrastructure/Database/Migrations`
- Create migrations: `dotnet ef migrations add [Name] --project src/Infrastructure --startup-project src/Web.Api -o Database/Migrations`

## Project Structure Guidelines

### Domain Layer
- Entities should inherit from appropriate base classes
- Implement domain events for important business actions
- Use value objects for complex types
- Define domain-specific errors

### Application Layer
- Commands/Queries should be simple DTOs
- Handlers contain business logic orchestration
- Use pipeline behaviors for cross-cutting concerns (logging, validation)
- Response DTOs for data transfer

### Infrastructure Layer
- External service integrations
- Database context and configurations
- Authentication/authorization setup

### Web.Api Layer
- Proper HTTP status code usage
- API versioning support
- Swagger/OpenAPI documentation

## Development Workflow

### Before Making Changes
1. **Analyze the complete project structure thoroughly before any other action.**
   - Identify the key source code and test directories for each layer (Domain, Application, Infrastructure, Web.Api, Unit Tests, Integration Tests). Do not assume standard folder names (e.g., check for `Endpoints` or `Controllers` or `Features`).
   - When assessing the completeness of a feature, you MUST verify the existence of its implementation in all relevant layers by listing the contents of its specific directories.
2. Understand the existing patterns in the affected layer
3. Check for similar existing implementations
4. Ensure changes align with Clean Architecture principles

### After Making Changes
1. Build the solution to verify no compilation errors
2. Run all tests to ensure nothing is broken
3. Verify code follows existing formatting standards
4. Check that domain logic stays in domain layer

### Database Changes
- Always create migrations for schema changes
- Test migrations both up and down
- Ensure data seeding works correctly

## Common Patterns to Follow

### Adding New Features
1. Start with domain entities and value objects
2. Add domain events if needed
3. Create command/query objects in Application layer
4. Implement handlers with proper error handling
5. Add infrastructure implementations if needed
6. Create API endpoints in Web.Api layer
7. Add comprehensive tests

### Error Handling Pattern
```csharp
// Use Result pattern instead of exceptions
public static class TodoItemErrors
{
    public static Error NotFound(TodoItemId id) => Error.NotFound(
        "TodoItem.NotFound",
        $"Todo item with ID '{id}' was not found.");
}
```

### CQRS Pattern (using Scrutor, NOT MediatR)
```csharp
// Commands for mutations
public record CreateTodoCommand(string Title, string Description) : ICommand<Result<TodoResponse>>;

// Queries for reads
public record GetTodoByIdQuery(TodoItemId Id) : IQuery<Result<TodoResponse>>;

// Handlers are registered via Scrutor assembly scanning and invoked through DI
// NO MediatR - use direct DI injection of ICommandHandler/IQueryHandler
```

## Logging and Monitoring
- Use Serilog for structured logging
- Seq available at http://localhost:8081 for log analysis
- Log important business events and errors
- Avoid logging sensitive information

## Testing Strategy
- Architecture tests to enforce layer dependencies
- Unit tests for application handler (IQueryHandler, ICommandHandler)
- Integration tests for API endpoints
- Mock external dependencies in tests

## Docker Support
- Docker Compose for local development
- PostgreSQL database container
- API container with hot reload support

Remember: This is a production-ready template focused on maintainability, testability, and following enterprise software development best practices.


# Architecture

The project includes comprehensive architecture tests and you must follow without change existing Architecture.Tests, and understand these below items as part of the requirements:

## Layer Dependency Rules
- **MANDATORY**: Domain layer cannot depend on Application, Infrastructure, or Presentation layers
- **MANDATORY**: Application layer cannot depend on Infrastructure or Presentation layers
- **MANDATORY**: Infrastructure layer cannot depend on Presentation layer
- **MANDATORY**: Proper Clean Architecture dependency flow validation

## Application Layer Rules
- **MANDATORY**: Commands must implement `ICommand` or `ICommand<T>` interfaces
- **MANDATORY**: Queries must implement `IQuery<T>` interface
- **MANDATORY**: Command handlers must implement `ICommandHandler<T>` or `ICommandHandler<T,R>` interfaces
- **MANDATORY**: Query handlers must implement `IQueryHandler<T,R>` interface
- **MANDATORY**: Queries should be record types for immutability
- **MANDATORY**: Handlers must return `Task<Result>` or `Task<Result<T>>` types
- **MANDATORY**: Vertical slice organization validation (features grouped by business capability)
- **MANDATORY**: No direct infrastructure dependencies
- **MANDATORY**: Only CachedRepository classes should use HybridCache for caching operations
- **MANDATORY**: CachedRepository classes must be located in `.Data` namespaces within feature slices
- **MANDATORY**: CachedRepository classes must NOT return Domain Entity types - use Response DTOs instead

## Domain Layer Rules
- **MANDATORY**: Domain entities must inherit from `Entity` base class
- **MANDATORY**: Domain events must implement `IDomainEvent` interface
- **MANDATORY**: Domain errors must be static classes
- **MANDATORY**: No infrastructure dependencies (pure domain logic)
- **MANDATORY**: Proper aggregate root patterns

## Infrastructure Layer Rules
- **MANDATORY**: Repository implementations is permitted, we use directly IReadonlyApplicationDbContext for IQueryHandler
- **MANDATORY**: Use `IApplicationDbContext` for ICommandHandler implementations
- **MANDATORY**: Infrastructure services must be internal (proper encapsulation)
- **MANDATORY**: DbContext must be internal
- **MANDATORY**: Only interfaces, extensions, configurations, constants, and enums should be public

## Presentation Layer Rules
- **MANDATORY**: Endpoints must implement `IEndpoint` interface
- **MANDATORY**: Endpoint classes must be internal
- **MANDATORY**: Proper API layer encapsulation

## Code Quality Rules
- **MANDATORY**: Async methods must have 'Async' suffix (except Handle and MapEndpoint methods)
- Public classes should have XML documentation
- Consistent naming conventions
- **MANDATORY**: Use file-scoped namespaces

## Testing Standards Rules
- **MANDATORY**: Unit tests should follow AAA (Arrange, Act, Assert) pattern
- **MANDATORY**: No EntityFramework InMemory usage in unit tests
- **MANDATORY**: NSubstitute for mocking (not Moq)
- **MANDATORY**: Proper test isolation and independence

## Directory and Feature Organization
- **MANDATORY**: Features must be grouped by business capability (e.g., Users, Todos, Roles) in all layers: Domain, Application, Infrastructure, Web.Api.
- For each feature, ensure the following files exist:
  - **Domain**: Entity, Domain Events, Domain Errors
  - **Application**: Command/Query, Handler, Validator, Response DTO, CachedRepository (in .Data namespace)
  - **Infrastructure**: Service implementations, repository, database context/configuration
  - **Web.Api**: Endpoint/controller
- Example vertical slice for `Users` feature:
  - `src/Domain/Users/User.cs`, `UserCreatedEvent.cs`, `UserErrors.cs`
  - `src/Application/Users/CreateUserCommand.cs`, `CreateUserHandler.cs`, `CreateUserValidator.cs`, `UserResponse.cs`, `Users.Data/UserCachedRepository.cs`
  - `src/Infrastructure/Users/UserRepository.cs`, `UserDbContextConfig.cs`
  - `src/Web.Api/Users/CreateUserEndpoint.cs`
- **MANDATORY**: When adding a feature, verify and list the contents of each relevant directory to ensure completeness.
