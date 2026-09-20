---
type: Pattern
title: "Cached Read"
description: "Put HybridCache in front of a projection, return DTOs, and invalidate after every write that changes the data."
resource: src/Application
tags: [caching, read, hybridcache, performance]
status: stable
---

# Cached Read

## Shape

One per slice, in `src/Application/<Feature>/Data/`: `I<Feature>CachedRepository`, its implementation, and
the response DTOs it returns. The implementation injects `IReadOnlyApplicationDbContext` and `HybridCache`;
each read is `cache.GetOrCreateAsync(key, factory)` around an `AsNoTracking` projection to a DTO, and each
cached key has a matching `RemoveCacheAsync`.

```csharp
public async Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken ct) =>
    await cache.GetOrCreateAsync($"users:{id}",
        async t => await db.Users.Where(u => u.Id == id)
            .Select(u => new UserResponse(u.Id, u.Email)).FirstOrDefaultAsync(t),
        cancellationToken: ct);

public ValueTask RemoveCacheAsync(Guid id, CancellationToken ct) => cache.RemoveAsync($"users:{id}", ct);
```

It is also the sanctioned way for one slice to read another slice's data -
[Vertical Slice Architecture](../../architecture/cross-cutting/vertical-slice-architecture.md).

## Invalidate on write

| Operation | Action |
|---|---|
| Create | Nothing - it was not cached |
| Read | Populate on miss |
| Update | Remove after `SaveChangesAsync` |
| Delete | Remove after `SaveChangesAsync` |

`RemoveCacheAsync` goes on the line after `SaveChangesAsync`. A forgotten invalidation is the most common caching bug here, and it presents as "the update worked but the
API still shows the old value", which people first blame on the browser.

## Cache this

Reference data, roles, profiles - frequently read, rarely changed, latency-sensitive.

## Do not cache this

* **Complex filtered queries** - the key space explodes and hit rate collapses.
* **Real-time figures** - counters, analytics, anything a user checks immediately after changing.
* **Rarely repeated queries** - you pay the write cost and never get a hit.

Query [`IReadOnlyApplicationDbContext`](../../../src/Application/Abstractions/Data/IReadOnlyApplicationDbContext.cs) directly instead.

## Rules

* **Return DTOs, never domain entities.** A cached entity is detached, stale and mutable.
* **Keys are `{entity}:{identifier}`** - `users:{userId}`, `roles:{roleId}` - lowercase, colon-separated, no
  sensitive data (keys reach Redis and logs: `users:{email}` is wrong), under 1024 characters, payloads
  under 1 MB. Build the key in the repository that owns it, never at the call site: a key written twice is
  eventually written two ways, and then the read hits a cache the write never clears. Group-invalidation tags
  live in [`CacheTags`](../../../src/Application/CacheTags.cs).
* Expiration: 2-5 minutes for changing data, 15-30 minutes for stable data.
* Without Redis, L1 is per-instance - another instance can serve stale data until it expires. See
  [Caching Tiers](../../architecture/cross-cutting/caching-tiers.md).
