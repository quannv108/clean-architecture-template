using System.Text.Json.Serialization;

namespace SharedKernel;

public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    [JsonInclude] public Guid Id { get; protected internal set; }

    [JsonInclude] public bool IsDeleted { get; protected internal set; }

    public uint Version { get; protected set; }

    public List<IDomainEvent> DomainEvents => [.. _domainEvents];

    #region Domain Events

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    #endregion

    #region Delete Event

    public virtual void RaiseDeleteEvent() { }

    #endregion
}
