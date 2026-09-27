namespace SharedKernel;

public abstract class AuditedEntity : Entity
{
    public DateTime CreatedAt { get; internal set; }
    public DateTime LastUpdated { get; internal set; }
    public DateTime? DeletedAt { get; internal set; }
    public Guid? CreatedBy { get; internal set; }
    public Guid? UpdatedBy { get; internal set; }
    public Guid? DeletedBy { get; internal set; }

    public DateTime LastUpdatedOrCreatedAt => LastUpdated > CreatedAt ? LastUpdated : CreatedAt;
}
