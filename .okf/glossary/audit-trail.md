---
type: Term
title: "Audit Trail"
description: "A durable record of actions taken through the system."
tags: [security, audit, compliance]
status: stable
---

# Audit Trail

Recorded by [`AuditLoggingMiddleware`](../../src/Web.Api/Middleware/AuditLoggingMiddleware.cs) for endpoints marked
`.WithAuditLog("...")`, following the [4W framework](four-w-framework.md).

Distinct from [`AuditedEntity`](../../src/SharedKernel/AuditedEntity.cs) stamps, which record who last changed a
**row**. The audit trail records an **action**, including reads - which is usually the part compliance cares
about.

See [Audit Logging](../engineering/patterns/audit-logging.md).
