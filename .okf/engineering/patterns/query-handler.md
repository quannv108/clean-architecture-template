---
type: Pattern
title: "Query Handler"
description: "The shape of a read use case: query record and handler in one file, projection to a DTO, no writes."
tags: [cqrs, queries, handlers, application]
status: stable
---

# Query Handler

A read use case is one file, `src/Application/<Feature>/<Operation>Query.cs`, holding the query record, the
response DTOs and the handler. It reads, projects and returns; it never writes.

## How

* The query is a `public sealed record : IQuery<TResponse>` with DataAnnotations; response DTOs are
  positional records in the same file.
* The handler is `internal sealed class <Operation>QueryHandler : IQueryHandler<...>` and returns
  `Result<TResponse>` - `Result.Failure(<Feature>Errors.NotFound(id))` when nothing matches.
* Choose the read path by the data, not by habit:

  | Data | Inject | Do |
  |---|---|---|
  | Hot, slow-changing, by id | `I<Feature>CachedRepository` - [Cached Read](cached-read.md) | Return what it gives you |
  | Filtered, ad hoc, large or real-time | `IReadOnlyApplicationDbContext` | `Where -> Select(new Dto(...)) -> ToListAsync` |

* **Project in the database.** `Select` into the DTO before materialising, so only the needed columns
  travel. Never return an entity or an `IQueryable`.
* Lists return [`PagedList<T>`](../../../src/Web.Api/Endpoints/Generic/PagedList.cs); large or append-only
  lists page by cursor.

```csharp
var items = await db.Orders
    .Where(o => o.CustomerId == query.CustomerId)
    .Select(o => new OrderResponse(o.Id, o.Reference, o.Status))   // DTO, in SQL
    .ToListAsync(ct);
```

Real instance: [`GetAuditLogsQuery.cs`](../../../src/Application/AuditLogs/GetAuditLogsQuery.cs)
(cursor-paged projection).

## Checklist

- [ ] `<Operation>Query.cs` holds the `sealed record`, the DTOs and the `internal sealed` handler
- [ ] Returns a DTO - never a domain entity, never an `IQueryable`
- [ ] Projects in the database (`Select` before materialising)
- [ ] Never calls `SaveChangesAsync`
- [ ] Lists return `PagedList<T>`

## When not to cache

Caching a filtered query explodes the key space and collapses the hit rate - [Cached Read](cached-read.md)
lists what to cache and what not to.
