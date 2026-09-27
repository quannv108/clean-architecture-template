using Domain.AuditLogs;
using Domain.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IReadOnlyApplicationDbContext
{
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
}
