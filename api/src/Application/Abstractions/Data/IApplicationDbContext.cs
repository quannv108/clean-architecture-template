using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext : IReadOnlyApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    DbSet<T> Set<T>() where T : class;
    DatabaseFacade Database { get; }
}
