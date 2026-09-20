---
type: Term
title: "Query"
description: "A request for data that changes nothing, as a record with a handler returning a DTO."
tags: [application, cqrs, read]
status: stable
---

# Query

A `sealed record` implementing [`IQuery<TResponse>`](../../src/Application/Abstractions/Messaging/IQuery.cs), declared with its handler.
Returns a response DTO - never a domain entity, never an `IQueryable`.

A query handler that calls `SaveChangesAsync` is a design error.

Shape: [Query Handler](../engineering/patterns/query-handler.md).
