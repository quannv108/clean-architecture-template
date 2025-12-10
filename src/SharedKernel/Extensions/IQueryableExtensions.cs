namespace SharedKernel.Extensions;

public static class IQueryableExtensions
{
    public static IQueryable<T> ForTenant<T>(this IQueryable<T> query, Guid tenantId)
        where T : ITenantEntity
    {
        return query.Where(e => e.TenantId == tenantId);
    }
}
