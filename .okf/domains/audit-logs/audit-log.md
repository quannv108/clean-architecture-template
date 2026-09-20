---
type: Entity
title: "AuditLog"
description: "One recorded action - who, what, when and where - immutable once created."
resource: src/Domain/AuditLogs/AuditLog.cs
tags: [domain, audit, entity, security]
status: stable
---

# AuditLog

| 4W | Property |
|---|---|
| Who | `UserId` - `Guid.Empty` for unauthenticated calls |
| What | `ActionName` - the business action from `.WithAuditLog(...)` |
| When | `ActionDateTime` - UTC |
| Where | `UrlPath`, `IpAddress`, `HttpResponseCode` |
| Extra | `AdditionalData` - JSON for business-specific context |

Inherits [`Entity`](../../../src/SharedKernel/Entity.cs), so soft delete applies and rows are never physically removed by
normal operations.

## Rules

* **Immutable after creation.** There is a create command and no update. Validation lives in the factory.
* **Never store sensitive data**, including in `AdditionalData`. Audit records are widely readable and long
  lived.
* Use descriptive business action names (`UserRegistration`, not `Post`) - somebody reads these during an
  incident.

Retention is enforced by
[`DeleteOldAuditLogsCommand`](../../../src/Application/AuditLogs/DeleteOldAuditLogsCommand.cs); that window is usually a
compliance decision.
