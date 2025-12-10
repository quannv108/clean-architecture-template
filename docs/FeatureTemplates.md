# Feature Templates

Quick reference for adding new features following vertical slice architecture.

## Feature Complexity Guide

Choose the appropriate template:

| Complexity | Operations | Examples | Template |
|------------|-----------|----------|----------|
| **Simple** | 1-2 basic CRUD | Profiles, Tenants | [Simple Template](#simple-feature-template) |
| **Medium** | 3-5 operations + business logic | Roles, Notifications | [Medium Template](#medium-feature-template) |
| **Complex** | 6+ operations, multiple sub-areas | Users, Authentication, Emails | [Complex Template](#complex-feature-template) |

## Simple Feature Template

**Use for**: Basic CRUD entities with minimal business logic (e.g., Profiles, Tenants)

### Structure
```
Domain/<Feature>/
  ├── <Feature>.cs
  └── <Feature>Errors.cs

Application/<Feature>/
  │   Create<Feature>CommandHandler.cs
  │   Get<Feature>QueryHandler.cs
  └── <Feature>PermissionsConstants.cs

Infrastructure/<Feature>/
  └── <Feature>Configuration.cs

Web.Api/Endpoints/<Feature>/
  ├── Create<Feature>.cs
  └── Get<Feature>.cs
```

### Files Required
- [ ] 2 Domain files (Entity + Errors)
- [ ] 2 Application files (2 operations: Create + Get)
- [ ] 1 Infrastructure file (EF Configuration)
- [ ] 2 Web.Api files (2 endpoints)
- [ ] 1 Permissions constants file

**Total**: ~6 files

## Medium Feature Template

**Use for**: Standard CRUD with additional business operations (e.g., Roles, Notifications)

### Structure
```
Domain/<Feature>/
  ├── <Feature>.cs
  ├── <Feature>Errors.cs
  ├── <Related>.cs                      # Related entities (e.g., RolePermission)
  └── <Event>DomainEvent.cs             # Optional: domain events

Application/<Feature>/
  │   Create<Feature>CommandHandler.cs
  │   Get<Feature>QueryHandler.cs
  │   Update<Feature>CommandHandler.cs
  │   Delete<Feature>CommandHandler.cs
  ├── Data/
  │   ├── I<Feature>CachedRepository.cs # both interface and implementation
  ├── Events/
  │   ├── <Event1>DomainEventHandler.cs
  └── <Feature>PermissionsConstants.cs

Infrastructure/<Feature>/
  ├── <Feature>Configuration.cs
  ├── <Related>Configuration.cs         # For related entities
  └── <Feature>Seeder.cs                # Optional: seed data

Web.Api/Endpoints/<Feature>/
  ├── Create<Feature>.cs
  ├── Get<Feature>.cs
  ├── Update<Feature>.cs
  └── Delete<Feature>.cs
```

### Files Required
- [ ] 3-4 Domain files (Entity + Related + Errors + Events)
- [ ] 4-7 Application files (4-5 operations + Data + Events)
- [ ] 2-3 Infrastructure files (Configurations + Seeder)
- [ ] 4-5 Web.Api files (CRUD endpoints)

**Total**: ~13-19 files

## Complex Feature Template

**Use for**: Features with multiple sub-areas or authentication flows (e.g., Users, Authentication, Emails)

### Structure
```
Domain/<Feature>/
  ├── <Feature>.cs
  ├── <Feature>Errors.cs
  ├── <Related1>.cs
  ├── <Related2>.cs
  ├── Events/
  │   ├── <Event1>DomainEventHandler.cs
  │   └── <Event2>DomainEventHandler.cs

Application/<Feature>/
  ├── <SubArea1>/                       # Logical grouping
  │   └── <Operation1>CommandHandler.cs
  │   └── <Operation2>CommandHandler.cs
  │   ├── I<SubArea1>CachedRepository.cs
  ├── <SubArea2>/
  │   └── <Operation3>CommandHandler.cs
  │   └── <Operation4>CommandHandler.cs
  │   ├── I<SubArea2>CachedRepository.cs
  ├── Events/
  │   ├── <Event1>DomainEventHandler.cs
  │   └── <Event2>DomainEventHandler.cs
  └── <Feature>PermissionsConstants.cs

Infrastructure/<Feature>/
  ├── <Feature>Configuration.cs
  ├── <Related1>Configuration.cs
  ├── <Related2>Configuration.cs
  ├── <Feature>Seeder.cs
  └── <SubFeature>/                     # Optional: sub-services
      └── <Service>.cs

Web.Api/Endpoints/<Feature>/
  ├── <Operation1>.cs
  ├── <Operation2>.cs
  ├── <SubArea1>/
  │   ├── <Operation3>.cs
  │   └── <Operation4>.cs
  └── <SubArea2>/
      └── <Operation5>.cs
```

### Files Required
- [ ] 5-8 Domain files (Multiple entities + Events)
- [ ] 7+ Application files (Multiple operations + Data + Events + Builders)
- [ ] 4-6 Infrastructure files (Multiple configurations + Seeders)
- [ ] 5+ Web.Api files (Multiple endpoints across sub-areas)

**Total**: ~20+ files

## Step-by-Step Creation Process

### 1. Domain Layer
1. Create `Domain/<Feature>/` folder
2. Add `<Feature>.cs` (entity inheriting from `Entity`)
3. Add `<Feature>Errors.cs` (static class with `Error` methods)
4. Add domain events if needed (`<Event>DomainEvent.cs` implementing `IDomainEvent`)
5. Add related entities if needed

### 2. Application Layer
1. Create `Application/<Feature>/` folder
2. For each operation:
   - Add Handler file (implements `ICommandHandler<T>` or `IQueryHandler<T,R>`)
   - Add Command or Query file (record type) to each CommandHandler file or QueryHandler file
3. Add CachedRepository (for read operations)
4. Add `Events/` folder with event handlers if needed
5. Add `<Feature>PermissionsConstants.cs`

### 3. Infrastructure Layer
1. Create `Infrastructure/<Feature>/` folder
2. Add `<Feature>Configuration.cs` (implements `IEntityTypeConfiguration<T>`)
3. Configure entity mapping, indexes, relationships
4. Add `<Feature>Seeder.cs` if seed data needed
5. Register seeder in `DbSeeder.cs`

### 4. Web.Api Layer
1. Create `Web.Api/Endpoints/<Feature>/` folder
2. For each endpoint:
   - Add `<Operation>.cs` file
   - Implement `IEndpoint` interface
   - Define `MapEndpoint` method with route, HTTP method, and handler invocation
3. Add nested folders for complex features

### 5. Tests Layer
1. Create `tests/Application.UnitTests/<Feature>/` folder
2. Add test class for each handler (`<Operation>HandlerTests.cs`)
3. Create `tests/Api.IntegrationTests/<Feature>/` folder
4. Add integration tests for endpoints

## Naming Checklist

Verify all files follow naming conventions:

- [ ] Domain: `<Feature>.cs`, `<Feature>Errors.cs`, `<Event>DomainEvent.cs`
- [ ] Commands: `<Operation>CommandHandler.cs`
- [ ] Queries: `<Operation>QueryHandler.cs`
- [ ] Repository: `I<Feature>CachedRepository.cs`
- [ ] EF Config: `<Entity>Configuration.cs`
- [ ] Endpoints: `<Operation>.cs` (e.g., `CreateRole.cs`, NOT `CreateRoleEndpoint.cs`)
- [ ] Tests: `<Operation>HandlerTests.cs`

## Mandatory Requirements

### Domain Layer
- [ ] Entities inherit from `Entity` base class
- [ ] Domain events implement `IDomainEvent`
- [ ] Domain errors are static classes returning `Error`
- [ ] NO infrastructure dependencies (pure domain logic)

### Application Layer
- [ ] Commands implement `ICommand` or `ICommand<T>`
- [ ] Queries implement `IQuery<T>`
- [ ] Handlers implement `ICommandHandler<T>` or `IQueryHandler<T,R>`
- [ ] Commands/Queries are record types
- [ ] Handlers return `Task<Result>` or `Task<Result<T>>`
- [ ] CachedRepository returns Response DTOs, NOT domain entities
- [ ] Use DataAnnotations for validation

### Infrastructure Layer
- [ ] Services and DbContext are internal
- [ ] Only interfaces, extensions, configurations, constants, and enums are public
- [ ] EF configurations implement `IEntityTypeConfiguration<T>`
- [ ] Use Options pattern (`IOptions<T>`) for configuration

### Web.Api Layer
- [ ] Endpoints implement `IEndpoint` interface
- [ ] Endpoint classes are internal
- [ ] Located in `Endpoints/<Feature>/` folder
- [ ] Use proper HTTP verbs and status codes

### Testing
- [ ] Unit tests follow AAA pattern (Arrange, Act, Assert)
- [ ] Use NSubstitute for mocking
- [ ] Integration tests use API endpoints only (NEVER write to database directly)
- [ ] Minimum 70% code coverage for Application layer

## Verification

After creating a new feature, verify:

1. **Build**: `dotnet build CleanArchitecture.slnx` (must succeed)
2. **Tests**: `dotnet test CleanArchitecture.slnx` (all pass)
3. **Architecture Tests**: Verify layer dependencies are correct
4. **Structure**: Compare with [VerticalSliceStructure.md](VerticalSliceStructure.md)
5. **Consistency**: Use checklist in VerticalSliceStructure.md

## Common Pitfalls

Avoid these mistakes:

- ❌ Using CachedRepository in command handlers (use DbContext directly)
- ❌ Returning domain entities from CachedRepository (use Response DTOs)
- ❌ Creating public DbContext (must be internal)
- ❌ Writing to database in integration tests (use API endpoints)
- ❌ Missing DataAnnotations validation on commands/queries
- ❌ Not implementing `IEndpoint` for Web.Api endpoints
- ❌ Creating endpoints as public classes (must be internal)

## Reference

- **Detailed Analysis**: [VerticalSliceStructure.md](VerticalSliceStructure.md)
- **Architecture Overview**: [Architecture.md](Architecture.md)
- **Code Generator**: `tools/CodeGenerator/` for automated scaffolding
