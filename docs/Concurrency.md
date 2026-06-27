# Concurrency

Three layers of concurrency control are available in this codebase. Choose based on what you're coordinating:

| Layer | Mechanism | Scope | Use When |
|---|---|---|---|
| **Optimistic concurrency** | PostgreSQL `xmin` | Single entity, DB-level | Concurrent entity updates (default) |
| **Atomic transactions** | EF Unit of Work / explicit transaction | Operation, DB-level | Multi-step write operations |
| **Distributed locks** | PostgreSQL advisory / Redis | Cross-instance, app-level | Background jobs, external API calls |

---

## Optimistic Concurrency

**Default choice for concurrent entity writes.** No overhead until a conflict occurs; no locks held between load and save.

PostgreSQL's `xmin` system column is mapped to `Entity.Version` as an EF row version token. EF automatically adds a `WHERE xmin = @version` clause to every UPDATE. If another write changed the row since it was loaded, the update affects 0 rows and EF throws `DbUpdateConcurrencyException`.

The `ConcurrencyExceptionDecorator` (in the decorator pipeline) catches this exception and returns `ConcurrencyErrors.UpdateConflict()`, which maps to HTTP 412. Callers can retry.

**Pattern:**
```csharp
// Load (xmin is tracked automatically)
var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);

// Mutate
order.Confirm();

// Save — EF checks xmin; throws DbUpdateConcurrencyException if row changed
await dbContext.SaveChangesAsync(ct);
// ConcurrencyExceptionDecorator converts the exception → Result.Failure(ConcurrencyErrors.UpdateConflict())
```

No extra code needed — the infrastructure is already wired.

---

## Atomic Transactions (Load → Calculate → Save)

**Rule of thumb: 1 load (all data upfront) → N in-memory mutations → 1 save.**

EF's `DbContext` is a Unit of Work. A single `SaveChangesAsync()` wraps all pending changes in one atomic DB transaction — including any `OutboxMessage` records created by domain events.

### When to use each pattern

**Default — EF Unit of Work (no explicit transaction):**
```csharp
var order = await dbContext.Orders
    .Include(o => o.Items)
    .FirstOrDefaultAsync(o => o.Id == id, ct);

order.ApplyDiscount(code);
order.Recalculate();

await dbContext.SaveChangesAsync(ct);  // atomic: entity + outbox messages
```

**Explicit transaction — only when multiple `SaveChanges()` calls must be atomic, or mixing EF with raw SQL:**
```csharp
await using var tx = await dbContext.Database.BeginTransactionAsync(ct);

var order = await dbContext.Orders.FindAsync(new object[] { orderId }, ct);
var inventory = await dbContext.Inventory.FindAsync(new object[] { productId }, ct);

order.Complete();
inventory.Deduct(order.Quantity);

await dbContext.SaveChangesAsync(ct);
await tx.CommitAsync(ct);
```

**`ExecuteUpdate`/`ExecuteDelete` — last resort only:**
```csharp
// Direct SQL — bypasses change tracking, domain events, and the Outbox
await dbContext.Orders
    .Where(o => o.Status == OrderStatus.Pending && o.CreatedAt < cutoff)
    .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, OrderStatus.Cancelled), ct);
```

Only use `ExecuteUpdate`/`ExecuteDelete` for operational/maintenance tasks where domain logic and domain events truly do not apply. Side effects (Outbox, audit log, cache invalidation) will silently not fire.

### Priority order

1. **EF Unit of Work** — default, no extra code
2. **Optimistic concurrency** — already wired, use by default for concurrent writes (see above)
3. **Explicit transaction** — multiple `SaveChanges()` or raw SQL
4. **`ExecuteUpdate`/`ExecuteDelete`** — avoid; only when proven necessary with no domain logic

### What to avoid

- Do not reach for pessimistic locks (`SELECT FOR UPDATE`) before trying optimistic concurrency.
- Do not split a logical load across multiple queries when one `Include()` covers the same data.
- Do not call `ExecuteUpdate`/`ExecuteDelete` in command handlers that raise domain events.

---

## Distributed Locks

For **cross-instance coordination** — preventing duplicate background job execution, serializing external API calls, or leader election. Not a substitute for optimistic concurrency on entity updates.

See [DistributedLock.md](DistributedLock.md) for configuration, usage patterns, provider comparison, and troubleshooting.

**Quick rule:** use optimistic concurrency for entity updates (automatic). Use distributed locks for background jobs and external API calls (manual).

---

## Choosing the Right Pattern

```
Concurrent entity writes?
  └─ Yes → Optimistic concurrency (xmin) — already wired, no extra code

Need multiple SaveChanges() in one atomic operation?
  └─ Yes → Explicit BeginTransactionAsync

Coordinating work across multiple app instances (background jobs, external APIs)?
  └─ Yes → Distributed lock (IDistributedLockProvider)

Bulk update with no domain logic, proven performance need?
  └─ Yes → ExecuteUpdate/ExecuteDelete (last resort)
```

---

**Related Documentation:**
- [DistributedLock.md](DistributedLock.md) — Full distributed locking reference
- [OutboxPattern.md](OutboxPattern.md) — How domain events are saved atomically with entities
- [Caching.md](Caching.md) — Read-side patterns (AsNoTracking + projection)
