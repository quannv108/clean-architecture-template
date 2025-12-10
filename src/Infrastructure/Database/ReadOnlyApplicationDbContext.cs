using Application.Abstractions.Data;
using Domain.AuditLogs;
using Domain.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

/// <summary>
/// Dedicated read-only DbContext intended for queries against a read replica.
/// - Uses NoTracking behavior by default.
/// - Does not expose SaveChanges via the IReadOnlyApplicationDbContext interface.
/// - Applies the same model configuration and global filters as the write context.
/// </summary>
internal sealed class ReadOnlyApplicationDbContext(DbContextOptions<ReadOnlyApplicationDbContext> options)
    : BaseApplicationDbContext(options), IReadOnlyApplicationDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Ensure no-tracking by default for all queries executed through this context
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        base.OnConfiguring(optionsBuilder);
    }
    public DbSet<AuditLog> AuditLogs { get; }
    public DbSet<OutboxMessage> OutboxMessages { get; }
}
