using Application.Abstractions.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SharedKernel;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Database.Interceptors;

internal sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUserContext _userContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditableEntityInterceptor(
        IDateTimeProvider dateTimeProvider,
        IUserContext userContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _dateTimeProvider = dateTimeProvider;
        _userContext = userContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private void UpdateAuditableEntities(DbContext context)
    {
        var utcNow = _dateTimeProvider.UtcNow;

        // Safely get the user ID, handling cases where there's no authenticated user
        Guid? userId = null;
        try
        {
            userId = _userContext.UserId;
        }
        catch (ApplicationException)
        {
            // Handle case where no authenticated user is available
            // Check if the current operation is allowed without authentication
            bool isAllowedWithoutAuth = IsOperationAllowedWithoutAuth();

            if (!isAllowedWithoutAuth)
            {
                // Operations not allowed without auth require an authenticated user
                throw new UnauthorizedAccessException("User authentication is required for this operation.");
            }

            // For operations allowed without auth, proceed without user context (userId remains null)
            // This enables creation of new entities without prior authentication
        }

        var entities = context.ChangeTracker.Entries<AuditedEntity>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .Select(entry => entry.Entity);

        foreach (var entity in entities)
        {
            var entry = context.Entry(entity);

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = utcNow;
                entity.CreatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                // Check if entity is being soft-deleted (IsDeleted changed from false to true)
                var isDeletedProperty = entry.Property(nameof(Entity.IsDeleted));
                if (isDeletedProperty is { OriginalValue: false, CurrentValue: true })
                {
                    entity.DeletedAt = utcNow;
                    entity.DeletedBy = userId;
                }
                else // If not soft-deleted, update LastUpdated and UpdatedBy
                {
                    // Only update LastUpdated and UpdatedBy if the entity is not being soft-deleted
                    // This prevents overwriting these fields during a soft delete operation
                    if (entity.DeletedAt == null)
                    {
                        entity.LastUpdated = utcNow;
                        entity.UpdatedBy = userId;
                    }
                }
            }
        }
    }

    private bool IsOperationAllowedWithoutAuth()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return false;
        }

        var path = httpContext.Request.Path.Value;
        var method = httpContext.Request.Method;

        if (path is null)
        {
            return false;
        }

        // List of endpoints allowed without authentication
        // These operations (e.g., registration) allow entity creation without user context
        var allowedEndpoints = new List<(string Method, string Path)>
        {
            ("POST", "/users/register"),
            // Add more endpoints allowed without auth here as needed
        };

        foreach (var (allowedMethod, allowedPath) in allowedEndpoints)
        {
            if (method.Equals(allowedMethod, StringComparison.OrdinalIgnoreCase) &&
                path.Contains(allowedPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
