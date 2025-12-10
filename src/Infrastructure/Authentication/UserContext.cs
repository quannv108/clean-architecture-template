using Application.Abstractions.Authentication;
using Domain;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private Guid? _overriddenUserId;
    private Guid? _overriddenTenantId;

    /// <summary>
    /// Get the user ID from the current HTTP context.
    /// Will return null if the user is not authenticated.
    /// Will return the overridden user ID if set.
    /// </summary>
    public Guid? UserId => _overriddenUserId ??
                           httpContextAccessor
                               .HttpContext?
                               .User
                               .GetUserId();

    /// <summary>
    /// Get the tenant ID from the current HTTP context.
    /// Will return null if the user is not authenticated or if the tenant ID is not set.
    /// Will return the overridden tenant ID if set.
    /// </summary>
    public Guid? TenantId => _overriddenTenantId ??
                             SystemConstants.UnknowTenantId; // TODO: update base on your approach

    public IDisposable OverrideUserId(Guid userId, Guid? tenantId = null)
    {
        return new UserContextScope(this, userId, tenantId);
    }

    private sealed class UserContextScope : IDisposable
    {
        private readonly UserContext _userContext;

        public UserContextScope(UserContext userContext, Guid userId, Guid? tenantId)
        {
            _userContext = userContext;
            _userContext._overriddenUserId = userId;
            _userContext._overriddenTenantId = tenantId;
        }

        public void Dispose()
        {
            _userContext._overriddenUserId = null;
            _userContext._overriddenTenantId = null;
        }
    }
}
