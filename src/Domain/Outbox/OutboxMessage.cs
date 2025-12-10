using System.Text.Json;
using SharedKernel;

namespace Domain.Outbox;

public sealed class OutboxMessage : Entity
{
    public string Type { get; private set; } = string.Empty;
    public string Content { get; private set; } = null!;
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public string? Error { get; private set; }
    public string? ProcessedByMachine { get; private set; }
    public OutboxMessageStatus Status { get; private set; }

    public Result<IDomainEvent> GetDomainEvent()
    {
        var assembly = typeof(OutboxMessage).Assembly;
        var type = assembly.GetType(Type);
        if (type == null)
        {
            return Result.Failure<IDomainEvent>(OutboxMessageErrors.DomainEventTypeNotFound(Type));
        }

        var domainEvent = (IDomainEvent?)JsonSerializer.Deserialize(Content, type);
        return domainEvent == null
            ? Result.Failure<IDomainEvent>(OutboxMessageErrors.UnableToDeserialize(Type, Id))
            : Result.Success(domainEvent);
    }

    // Private constructor to enforce factory method usage
    private OutboxMessage() { }

    public static OutboxMessage Create(IDomainEvent domainEvent, DateTime occurredOnUtc)
    {
        var item = new OutboxMessage
        {
            Id = EntityIdGenerator.NewId(),
            Type = domainEvent.GetType().FullName!,
            Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
            OccurredOnUtc = occurredOnUtc,
            Status = OutboxMessageStatus.Pending
        };
        return item;
    }

    public void MarkAsProcessing(string processedByMachine)
    {
        ProcessedByMachine = processedByMachine;
        Status = OutboxMessageStatus.Processing;
    }

    public void MarkAsProcessed(DateTime processedOnUtc, string processedByMachine)
    {
        ProcessedOnUtc = processedOnUtc;
        ProcessedByMachine = processedByMachine;
        Status = OutboxMessageStatus.Processed;
    }

    public void SetError(string error)
    {
        Error = error;
        Status = OutboxMessageStatus.Failed;
    }
}

public enum OutboxMessageStatus
{
    Pending = 0,
    Processing = 3,
    Processed = 1,
    Failed = 2
}
