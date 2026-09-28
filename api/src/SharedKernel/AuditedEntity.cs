namespace SharedKernel;

public abstract class AuditedEntity : Entity
{
    public DateTimeOffset CreatedAt { get; internal set; }
    public DateTimeOffset LastUpdated { get; internal set; }
    public DateTimeOffset? DeletedAt { get; internal set; }
    public Guid? CreatedBy { get; internal set; }
    public Guid? UpdatedBy { get; internal set; }
    public Guid? DeletedBy { get; internal set; }

    public DateTimeOffset LastUpdatedOrCreatedAt => LastUpdated > CreatedAt ? LastUpdated : CreatedAt;
}
