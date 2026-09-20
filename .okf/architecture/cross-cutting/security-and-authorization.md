---
type: Mechanism
title: Security and Authorization
description: Identity through IUserContext, per-feature permission constants, rate-limit and CORS policies, and what the audit trail records.
tags: [security, authorization, permissions, cors, rate-limiting]
status: stable
---

# Security and Authorization

## Identity

[`IUserContext`](../../../src/Application/Abstractions/Authentication/IUserContext.cs) (Application) is the only way to ask who the caller is.
[`UserContext`](../../../src/Infrastructure/Authentication/UserContext.cs) (Infrastructure) reads it from the `ClaimsPrincipal` via
[`ClaimsPrincipalExtensions`](../../../src/Infrastructure/Authentication/ClaimsPrincipalExtensions.cs). Handlers never touch
`HttpContext`.

Background work has no caller. Domain event handlers and jobs run under the system context - see
[`SystemConstants`](../../../src/Domain/SystemConstants.cs).

## Permissions

Each slice declares `Application/<Feature>/<Feature>PermissionsConstants.cs` - string constants naming the
permissions that slice recognises. Endpoints require them through the authorization policy in the builder
chain. Keeping the constants in Application means a handler and an endpoint can never disagree about a
permission's name.

## Policies configured in Web.Api

| Concern | Where |
|---|---|
| CORS | `Extensions/Cors/` + [`CorsPolicyNameConstants`](../../../src/Web.Api/Extensions/Cors) |
| Rate limiting | `Extensions/RateLimits/` + [`RateLimitPolicyNameConstants`](../../../src/Web.Api/Extensions/RateLimits) |
| Unhandled exceptions | [`GlobalExceptionHandler`](../../../src/Web.Api/Infrastructure/GlobalExceptionHandler.cs) |

Policy names are constants, never inline strings - a typo in a policy name fails open.

* **CORS** origins come from configuration, so a permissive development policy cannot leak into a deployed
  environment without a config change.
* **Rate limits** are applied per endpoint in the builder chain; put stricter policies on anything that costs
  money per call (mail, SMS, authentication). Limits are **per instance** unless backed by a shared store -
  in a multi-instance deployment the effective limit is per-instance x instance count.

## Audit trail

Security-relevant endpoints are marked `.WithAuditLog("ActionName")`;
[`AuditLoggingMiddleware`](../../../src/Web.Api/Middleware/AuditLoggingMiddleware.cs) records Who, What, When and Where
including unauthenticated calls. See [Audit Logging](../../engineering/patterns/audit-logging.md).

## Data protection

Sensitive columns use [`EncryptedString`](../../../src/SharedKernel/EncryptedString.cs) with versioned AES-256 keys -
[Field Encryption and Key Rotation](../../engineering/patterns/encryption.md). Never put secrets, tokens or personal data in log
messages, cache keys, lock names or audit `AdditionalData`.
