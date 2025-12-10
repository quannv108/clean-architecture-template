namespace Application.Abstractions.Authentication;

public interface IUserContext
{
    Guid? UserId { get; }
    Guid? TenantId { get; }

    IDisposable OverrideUserId(Guid userId, Guid? tenantId = null);
}
