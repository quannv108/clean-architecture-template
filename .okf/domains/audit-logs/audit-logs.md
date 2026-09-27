---
type: Domain Slice
title: "Audit Logs"
description: "The 4W audit trail: who did what, when and where, recorded for endpoints marked for auditing."
resource: api/src/Domain/AuditLogs
tags: [domain, audit, security, compliance]
status: stable
---

# Audit Logs

Records actions taken through the API - not row changes, which are covered by
[`AuditedEntity`](../../../api/src/SharedKernel/AuditedEntity.cs) stamps.

## Across the layers

| Layer | Files |
|---|---|
| Domain | [`AuditLog`](audit-log.md), [`AuditLogErrors`](audit-log-errors.md) |
| Application | [create command](../../../api/src/Application/AuditLogs/CreateAuditLogCommand.cs), [query](../../../api/src/Application/AuditLogs/GetAuditLogsQuery.cs), [retention command](../../../api/src/Application/AuditLogs/DeleteOldAuditLogsCommand.cs) and [job](../../../api/src/Application/AuditLogs/DeleteOldAuditLogsBackgroundJob.cs) |
| Infrastructure | [`AuditLogConfiguration`](../../../api/src/Infrastructure/Database/Configuration/AuditLogs/AuditLogConfiguration.cs) with its `UserId` and `ActionDateTime` indexes |
| Web.Api | [`AuditAttribute`](../../../api/src/Web.Api/Infrastructure/AuditAttribute.cs), [`.WithAuditLog(...)`](../../../api/src/Web.Api/Extensions/AuditLogs/RouteHandlerBuilderExtensions.cs), [`AuditLoggingMiddleware`](../../../api/src/Web.Api/Middleware/AuditLoggingMiddleware.cs), [the query endpoint](../../../api/src/Web.Api/Endpoints/AuditLogs/GetAuditLogs.cs) |

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

Implementation: [`api/src/Application/AuditLogs`](../../../api/src/Application/AuditLogs) and [`api/src/Web.Api/Middleware/AuditLoggingMiddleware.cs`](../../../api/src/Web.Api/Middleware/AuditLoggingMiddleware.cs); mechanism in [Audit Logging](../../engineering/patterns/audit-logging.md).
