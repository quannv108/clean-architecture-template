---
type: Domain Slice
title: "Audit Logs"
description: "The 4W audit trail: who did what, when and where, recorded for endpoints marked for auditing."
resource: src/Domain/AuditLogs
tags: [domain, audit, security, compliance]
status: stable
---

# Audit Logs

Records actions taken through the API - not row changes, which are covered by
[`AuditedEntity`](../../../src/SharedKernel/AuditedEntity.cs) stamps.

## Across the layers

| Layer | Files |
|---|---|
| Domain | [`AuditLog`](audit-log.md), [`AuditLogErrors`](audit-log-errors.md) |
| Application | [create command](../../../src/Application/AuditLogs/CreateAuditLogCommand.cs), [query](../../../src/Application/AuditLogs/GetAuditLogsQuery.cs), [retention command](../../../src/Application/AuditLogs/DeleteOldAuditLogsCommand.cs) and [job](../../../src/Application/AuditLogs/DeleteOldAuditLogsBackgroundJob.cs) |
| Infrastructure | [`AuditLogConfiguration`](../../../src/Infrastructure/Database/Configuration/AuditLogs/AuditLogConfiguration.cs) with its `UserId` and `ActionDateTime` indexes |
| Web.Api | [`AuditAttribute`](../../../src/Web.Api/Infrastructure/AuditAttribute.cs), [`.WithAuditLog(...)`](../../../src/Web.Api/Extensions/AuditLogs/RouteHandlerBuilderExtensions.cs), [`AuditLoggingMiddleware`](../../../src/Web.Api/Middleware/AuditLoggingMiddleware.cs), [the query endpoint](../../../src/Web.Api/Endpoints/AuditLogs/GetAuditLogs.cs) |

## Using it

Mark an endpoint and nothing else is required:

```csharp
app.MapPost("users/register", HandleAsync)
    .WithAuditLog("UserRegistration")
    .WithTags(Tags.Users);
```

Unauthenticated endpoints are audited too, with `Guid.Empty` as the user.

Full behaviour, including the fire-and-forget trade-off and retention:
[Audit Logging](../../engineering/patterns/audit-logging.md).

## Files in this slice

* [AuditLog](audit-log.md) - One recorded action - who, what, when and where - immutable once created.
* [AuditLogErrors](audit-log-errors.md) - Domain error factories for audit log validation failures.

Implementation: [`src/Application/AuditLogs`](../../../src/Application/AuditLogs) and [`src/Web.Api/Middleware/AuditLoggingMiddleware.cs`](../../../src/Web.Api/Middleware/AuditLoggingMiddleware.cs); mechanism in [Audit Logging](../../engineering/patterns/audit-logging.md).
