namespace Domain;

public static class SystemConstants
{
    /// <summary>
    /// System tenant ID used for super admin and internal system operations.
    /// </summary>
    public static readonly Guid SystemTenantId = new("00000000-0000-0000-0000-000000000001");

    public static readonly Guid UnknowTenantId = Guid.Empty;


    public static readonly Guid AnonymousUserId = new("00000000-0000-0000-0000-000000000001");
    public static readonly Guid SystemUserId = new("00000000-0000-0000-0000-000000000002");
}
