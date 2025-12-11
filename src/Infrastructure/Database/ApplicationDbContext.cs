using Application.Abstractions.Data;
using Domain.AuditLogs;
using Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Database;

internal sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : BaseApplicationDbContext(options), IApplicationDbContext
{
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // When should you publish domain events?
        //
        // 1. BEFORE calling SaveChangesAsync
        //     - domain events are part of the same transaction
        //     - immediate consistency
        // 2. AFTER calling SaveChangesAsync
        //     - domain events are a separate transaction
        //     - eventual consistency
        //     - handlers can fail

        var domainEvents = ExtractPendingDomainEvents();
        await StoreDomainEventsAsync(domainEvents);

        int result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }

    private List<IDomainEvent> ExtractPendingDomainEvents()
    {
        var domainEvents = ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                List<IDomainEvent> domainEvents = entity.DomainEvents;

                entity.ClearDomainEvents();

                return domainEvents;
            })
            .ToList();
        return domainEvents;
    }

    private Task StoreDomainEventsAsync(List<IDomainEvent> domainEvents)
    {
        if (domainEvents.Count == 0)
        {
            return Task.CompletedTask;
        }

        foreach (var domainEvent in domainEvents)
        {
            var outboxMessage = OutboxMessage.Create(
                domainEvent,
                DateTime.UtcNow);

            OutboxMessages.Add(outboxMessage);
        }

        return Task.CompletedTask;
    }
}
