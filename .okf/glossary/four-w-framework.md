---
type: Term
title: "4W Framework"
description: "Who, What, When, Where - the four facts every audit entry records."
tags: [security, audit, compliance]
status: stable
---

# 4W Framework

| W | Recorded as |
|---|---|
| **Who** | `UserId`, or `Guid.Empty` when unauthenticated |
| **What** | The action name from `.WithAuditLog(...)` |
| **When** | UTC timestamp |
| **Where** | URL path, IP address, HTTP response code |

Plus `AdditionalData` for business context. **Never sensitive data** - audit records are widely readable and
long lived. See [AuditLog](../domains/audit-logs/audit-log.md).
