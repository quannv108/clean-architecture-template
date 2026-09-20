---
type: Term
title: "Soft Delete"
description: "Marking a row deleted instead of removing it, and hiding it with a query filter."
tags: [data, persistence, data-retention]
status: stable
---

# Soft Delete

Every [`Entity`](../../src/SharedKernel/Entity.cs) has `IsDeleted`, and a
[global query filter](global-query-filter.md) hides flagged rows from ordinary queries.

Keeps history and foreign keys intact, and makes deletion reversible. Costs: tables grow, unique
constraints must account for hidden rows, and anything reading the database outside EF Core does not get
the filter.

See [Soft Delete](../engineering/patterns/soft-delete.md).
