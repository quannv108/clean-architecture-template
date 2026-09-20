---
type: Term
title: "Multi-Tenancy"
description: "Serving multiple customers from one deployment with their data kept separate."
tags: [security, multi-tenancy, data]
status: stable
---

# Multi-Tenancy

Supported by [`ITenantEntity`](../../src/SharedKernel/ITenantEntity.cs) and a tenant
[global query filter](global-query-filter.md), so a query without an explicit tenant predicate is still
scoped.

Treat the filter as the safety net, not the design: write handlers as if a row from another tenant could
never be returned. If your product is single-tenant, entities simply do not implement the interface.
