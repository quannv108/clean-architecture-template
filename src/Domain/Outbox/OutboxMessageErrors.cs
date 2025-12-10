using SharedKernel;

namespace Domain.Outbox;

public static class OutboxMessageErrors
{
    public static Error DomainEventTypeNotFound(string type) =>
        Error.Problem("OutboxMessage.DomainEventTypeNotFound", $"Domain event type '{type}' not found.");

    public static Error UnableToDeserialize(string type, Guid id) =>
        Error.Problem("OutboxMessage.UnableToDeserialize",
            $"Unable to deserialize domain event of type '{type}' of outbox message {id}.");
}
