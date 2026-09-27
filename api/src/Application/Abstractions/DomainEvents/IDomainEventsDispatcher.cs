using SharedKernel;

namespace Application.Abstractions.DomainEvents;

/// <summary>
/// Dispatch domain events asynchronously via memory.
/// This is not store to database so just use it for not important messages
/// </summary>
public interface IDomainEventsDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
