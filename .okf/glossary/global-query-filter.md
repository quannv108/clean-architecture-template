---
type: Term
title: "Global Query Filter"
description: "An EF Core predicate applied automatically to every query for an entity type."
tags: [data, ef-core, persistence]
status: stable
---

# Global Query Filter

Configured on the model, so it applies without any query mentioning it. Used here for
[soft delete](soft-delete.md) and for tenant scoping
([`ITenantEntity`](../../src/SharedKernel/ITenantEntity.cs)).

`IgnoreQueryFilters()` bypasses it - deliberately, for administrative queries, and never in a normal read
path, because it silently returns deleted or cross-tenant rows.
