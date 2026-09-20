---
type: Domain Errors
title: "AuditLogErrors"
description: "Domain error factories for audit log validation failures."
resource: src/Domain/AuditLogs/AuditLogErrors.cs
tags: [domain, audit, errors]
status: stable
---

# AuditLogErrors

Error factories for [`AuditLog`](audit-log.md) creation and query failures - a missing action name, an
invalid time range.

It lives in `Domain/AuditLogs/` because it relates to a domain entity. Putting it in Application or Web.Api
fails `tests/ArchitectureTests` - see
[Error Codes](../../engineering/conventions/error-codes.md).
