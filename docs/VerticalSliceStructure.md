# Vertical Slice Structure

Complete analysis of all features and their organization across layers in this Clean Architecture project.

## Standard Folder Structure

Every feature follows this pattern (scaled to complexity):

```
<Feature>/
├── Domain/<Feature>/
│   ├── <Feature>.cs                    # Main entity
│   ├── <Feature>Errors.cs              # Domain errors (static class)
│   ├── <Related>.cs                    # Related entities
│   └── *DomainEvent.cs                 # Domain events
│
├── Application/<Feature>/
│   │   ├── <Operation>CommandHandler.cs
│   │   ├── <Operation>QueryHandler.cs
│   ├── Data/                           # CachedRepository (reads only)
│   │   ├── I<Feature>CachedRepository.cs # interface, implement, dto
│   ├── Events/                         # Domain event handlers
│   └── <Feature>PermissionsConstants.cs
│
├── Infrastructure/<Feature>/
│   ├── <Feature>Configuration.cs       # EF Core config
│   └── <Feature>Seeder.cs             # Seeding (optional)
│
└── Web.Api/Endpoints/<Feature>/
    ├── <Operation>.cs                  # IEndpoint implementation
    └── <SubFeature>/                   # Nested (for complex features)
        └── <SubOperation>.cs
```

## Feature Complexity Patterns

### Simple Features
- **Structure**: 1-2 operations, minimal logic
- **Examples**: Profiles, Tenants, AuditLogs
- **Pattern**:
  ```
  Domain/<Feature>/<Feature>.cs, <Feature>Errors.cs
  Application/<Feature>/<Operation1>/, <Operation2>/
  Infrastructure/<Feature>/<Feature>Configuration.cs
  Web.Api/Endpoints/<Feature>/<Operation1>.cs, <Operation2>.cs
  ```

### Medium Features
- **Structure**: 3-5 operations, standard CRUD + business logic
- **Examples**: Roles, Notifications, PushNotifications
- **Pattern**: Standard structure + Data folder + Events (optional)

### Complex Features
- **Structure**: 6+ operations, multiple sub-areas, nested organization
- **Examples**: Users, Authentication, Emails
- **Pattern**: Standard structure + nested subfolders for logical grouping
  ```
  Application/<Feature>/
    ├── <SubArea1>/
    │   ├── <Operation1>/
    │   └── <Operation2>/
    ├── <SubArea2>/
    │   └── <Operation3>/
    └── Data/
  ```
- **Real Examples**:
  - **Users**: AddRole/, RemoveRole/, EmailConfirmation/Confirm/, EmailConfirmation/ReSend/
  - **Authentication**: EmailCodeLogin/, EmailPassword/, PhoneCodeLogin/, Password/, RefreshTokens/
  - **Emails**: Builders/, Delivery/, Templates/, UserEmails/

## Class/Record/Interface Naming Conventions

| Element | Pattern | Example |
|---------|---------|---------|
| Domain Entity | `<Feature>.cs` | `User.cs`, `Role.cs` |
| Domain Errors | `<Feature>Errors.cs` | `UserErrors.cs`, `RoleErrors.cs` |
| Domain Event | `<Event>DomainEvent.cs` | `UserRegisteredDomainEvent.cs` |
| Command | `<Operation>Command.cs` | `CreateRoleCommand.cs` |
| Query | `<Operation>Query.cs` | `GetUserByIdQuery.cs` |
| Handler | `<Operation>CommandHandler.cs` | `CreateRoleCommandHandler.cs` |
| Response DTO | `<Operation>Response.cs` or `<Feature>Response.cs` | `UserResponse.cs` |
| CachedRepository | `<Feature>CachedRepository.cs` | `UserCachedRepository.cs` |
| Repository Interface | `I<Feature>CachedRepository.cs` | `IUserCachedRepository.cs` |
| EF Configuration | `<Entity>Configuration.cs` | `UserConfiguration.cs` |
| Seeder | `<Feature>Seeder.cs` | `UserSeeder.cs` |
| Endpoint | `<Operation>.cs` | `CreateRole.cs`, `GetUserById.cs` |
| Permissions | `<Feature>PermissionsConstants.cs` | `UserPermissionsConstants.cs` |

## Data Access Patterns

### Reads (Queries)
- **Location**: `Application/<Feature>/Data/`
- **Pattern**: CachedRepository classes with HybridCache
- **Returns**: Response DTOs (NEVER domain entities)
- **Interface**: `I<Feature>CachedRepository`
- **Implementation**: `<Feature>CachedRepository`

### Writes (Commands)
- **Location**: Command handlers use DbContext directly
- **Pattern**: Inject `IApplicationDbContext`
- **No caching**: Direct database writes

## Architecture Validation

### Layer Dependencies (Enforced by Architecture Tests)
```
Domain: NO dependencies (pure domain logic)
Application: Domain + SharedKernel ONLY
Infrastructure: Application + Domain + SharedKernel
Web.Api: ALL layers
```

### Feature Consistency Checklist

For each feature, verify:
- [ ] Domain entity exists in `Domain/<Feature>/`
- [ ] Domain errors exist in `Domain/<Feature>/<Feature>Errors.cs`
- [ ] Commands/Queries in `Application/<Feature>/`
- [ ] CachedRepository in `Application/<Feature>/Data/` (for reads)
- [ ] EF configuration in `Infrastructure/<Feature>/<Feature>Configuration.cs`
- [ ] Endpoints in `Web.Api/Endpoints/<Feature>/`
- [ ] Unit tests in `tests/Application.UnitTests/<Feature>/`
- [ ] Integration tests in `tests/Api.IntegrationTests/<Feature>/`
- [ ] Permissions constants defined (if secured)
- [ ] All handlers return `Result` or `Result<T>`
- [ ] All endpoints implement `IEndpoint`

## Cross-Feature Communication

Features communicate through well-defined boundaries:

| Method | Use Case | Example |
|--------|----------|---------|
| **Domain Events** | Async, decoupled notifications | `UserRegisteredDomainEvent` |
| **CachedRepository** | Read data from other features | Orders query Users via `IUserCachedRepository` |
| **Shared DTOs** | Public Response objects | `UserResponse`, `RoleResponse` |

**Anti-Patterns**:
- ❌ Direct handler-to-handler calls
- ❌ Feature A modifying Feature B's entities
- ❌ Shared domain entities across features

## Quick Reference

- **Architecture Overview**: See [Architecture.md](Architecture.md)
- **Adding New Features**: See [FeatureTemplates.md](FeatureTemplates.md)
- **Domain Events**: See [DomainEvent.md](DomainEvent.md)
- **Code Generator**: `dotnet run --project tools/CodeGenerator -- gen-entity -n <EntityName>`
