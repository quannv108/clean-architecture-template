---
type: Pattern
title: "Audit Logging"
description: "Record who did what, when and where by marking an endpoint - non-intrusive, asynchronous, and never able to fail a request."
resource: src/Web.Api/Middleware/AuditLoggingMiddleware.cs
tags: [audit, security, compliance, middleware]
status: stable
---

# Audit Logging

## Marking an endpoint

```csharp
app.MapGet("users/{userId:guid}", HandleAsync)
    .RequireAuthorization()
    .WithAuditLog("GetUserById")
    .WithTags(Tags.Users);

app.MapPost("users/register", HandleAsync)
    .WithAuditLog("UserRegistration")     // works unauthenticated too
    .WithTags(Tags.Users);
```

That is the entire developer-facing API. [`AuditLoggingMiddleware`](../../../src/Web.Api/Middleware/AuditLoggingMiddleware.cs)
does the rest.

## What is recorded

| 4W | Value |
|---|---|
| **Who** | User id, or `Guid.Empty` when unauthenticated |
| **What** | The action name you passed |
| **When** | UTC timestamp |
| **Where** | URL path, IP address, HTTP response code |

IP resolution honours `X-Forwarded-For`, `X-Real-IP` and `CF-Connecting-IP`, with IPv6 support. Extra
business context goes in `AdditionalData` as JSON.

## Querying it

```http
GET /api/v1/auditlogs?userId={guid}&take=50
GET /api/v1/auditlogs?actionName=UserLogin&fromDateTime=2026-01-01T00:00:00Z
```

Cursor-paged on `ActionDateTime`, backed by indexes on `UserId` and `ActionDateTime`.

## The trade-off to understand

Writing is **fire-and-forget**: the audit write happens after the response is produced, and a failure is
logged but never fails the request. That keeps auditing from becoming an availability risk - and it means
**the audit trail is not guaranteed complete**. If a regulation requires it to be, the write has to move
inside the request's transaction, which is a different design and a different failure mode.

## Rules

* Mark every security-relevant endpoint - authentication, permission changes, data export, admin actions.
* Use descriptive business action names (`UserRegistration`, not `Post`).
* **Never put sensitive data in audit records**, including `AdditionalData`. They are widely readable and
  long lived.
* Records are immutable - create only, never update. Deletion happens only through
  [retention](../../../src/Application/AuditLogs/DeleteOldAuditLogsCommand.cs), and that window is usually a compliance
  decision.

## Not the same as entity audit stamps

[`AuditedEntity`](../../../src/SharedKernel/AuditedEntity.cs) records who last changed a **row**. This records an
**action taken through the API**, whether or not it changed anything - including reads, which is usually the
part compliance cares about.
