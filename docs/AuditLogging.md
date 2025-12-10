# Audit Logging System

## Overview

Comprehensive user action tracking implementing the 4W audit framework: **Who**, **What**, **When**, and **Where**. Follows Clean Architecture principles with non-intrusive, declarative syntax.

## Design Principles

- **Non-intrusive**: Doesn't affect business logic or performance
- **Declarative**: Simple `.WithAuditLog("ActionName")` syntax
- **Consistent**: Works for authenticated and unauthenticated endpoints
- **Asynchronous**: Fire-and-forget pattern prevents blocking
- **Clean Architecture Compliant**: Infrastructure concerns in presentation layer

## Architecture

**Layer Distribution:**
- **Domain**: `AuditLog` entity (4W properties), `AuditLogErrors`
- **Application**: `CreateAuditLogCommand`, `GetAuditLogsQuery` with handlers
- **Infrastructure**: EF Core configuration, permission constants, database indexes
- **Web.Api**: `AuditAttribute`, `WithAuditLog()` extension, middleware

**Key Components:**
- Entity properties: `UserId`, `ActionName`, `ActionDateTime`, `UrlPath`, `IpAddress`, `HttpResponseCode`, `AdditionalData`
- Inherits from `Entity` for soft delete support
- Immutable after creation with factory method validation

## Usage

### Mark Endpoints for Audit

```csharp
// Add .WithAuditLog("ActionName") to any endpoint
app.MapGet("users/{userId:guid}", handler)
    .RequireAuthorization()
    .WithAuditLog("GetUserById")  // ← Audit marking
    .WithTags(Tags.Users);

// Works for unauthenticated endpoints too
app.MapPost("users/register", handler)
    .WithAuditLog("UserRegistration")
    .WithTags(Tags.Users);
```

### Query Audit Logs

```http
GET /auditlogs?userId={guid}&take=50
GET /auditlogs?actionName=UserLogin&fromDateTime=2025-01-01T00:00:00Z
```

## Security & Permissions

**IP Address Capture** supports `X-Forwarded-For`, `X-Real-IP`, `CF-Connecting-IP` headers, with IPv6 support.

**Unauthenticated Requests** use `Guid.Empty` as user identifier while capturing IP, action, URL, and response code.

## Performance

- **Asynchronous**: Fire-and-forget pattern ensures audit failures never block requests
- **Database**: Indexed on `UserId` and `ActionDateTime` for optimal query performance
- **Pagination**: Cursor-based using `ActionDateTime` (not page-based)
- **Error Handling**: Failed audits logged separately; system degrades gracefully

## Compliance

**4W Framework:**
- WHO: User ID (or `Guid.Empty` for anonymous)
- WHAT: Action name (business operation)
- WHEN: UTC timestamp
- WHERE: URL path + IP address + HTTP response code

**Data Retention:**
- Soft delete (never physically deleted)
- Immutable records (no updates after creation)
- All operations audited including permission changes

## Best Practices

**DO:**
- Mark security-critical endpoints with `.WithAuditLog("ActionName")`
- Use descriptive action names
- Monitor audit pipeline health
- Test audit functionality in integration tests

**DON'T:**
- Store sensitive data in audit logs
- Modify audit records after creation
- Bypass audit logging for admin operations

## Extension

**Custom Audit Data:** Use `AdditionalData` field for business-specific context (JSON serialized).

**Middleware Position:** After authentication/authorization for user context access.

**Reference:** See `AuditLogPermissionsConstants`, `CreateAuditLogCommand`, `GetAuditLogsQuery`, and `AuditLoggingMiddleware` implementations.
