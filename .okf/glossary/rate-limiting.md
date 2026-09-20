---
type: Term
title: "Rate Limiting"
description: "Capping how often a caller may invoke an endpoint."
tags: [http, security, performance]
status: stable
---

# Rate Limiting

Configured in `Web.Api/Extensions/RateLimits/` with names in `RateLimitPolicyNameConstants`, applied per
endpoint.

Apply a stricter policy to expensive or sensitive endpoints - authentication, sending mail or SMS, anything
that costs money per call.

**Limits are per instance** unless backed by a shared store, so across N instances the effective limit is N
times the configured one.
