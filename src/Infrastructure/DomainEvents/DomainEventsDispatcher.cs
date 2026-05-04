#pragma warning disable CA1873
using System.Collections.Concurrent;
using Application.Abstractions.Authentication;
using Application.Abstractions.DomainEvents;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Infrastructure.DomainEvents;

internal sealed class DomainEventsDispatcher(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<DomainEventsDispatcher> logger)
    : IDomainEventsDispatcher
{
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeDictionary = new();
    private static readonly ConcurrentDictionary<Type, Type> WrapperTypeDictionary = new();

    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();

            // All domain event handlers run as system — set audit context for AuditableEntityInterceptor
            var userContext = scope.ServiceProvider.GetRequiredService<IUserContext>();
            using var _ = userContext.OverrideUserId(SystemConstants.SystemUserId);

            Type domainEventType = domainEvent.GetType();
            Type handlerType = HandlerTypeDictionary.GetOrAdd(
                domainEventType,
                et => typeof(IDomainEventHandler<>).MakeGenericType(et));

            var handlerList = scope.ServiceProvider.GetServices(handlerType).ToList();

            if (handlerList.Count == 0)
            {
                logger.LogInformation("No handler registered for domain event {DomainEventType}", domainEventType.Name);
                continue;
            }

            foreach (object? handler in handlerList)
            {
                if (handler is null)
                {
                    continue;
                }

                try
                {
                    var handlerWrapper = HandlerWrapper.Create(handler, domainEventType);

                    await handlerWrapper.Handle(domainEvent, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error dispatch domain event {DomainEventType} with handler {HandlerType}",
                        domainEventType.Name, handler.GetType().Name);
                }
            }
        }
    }

    private abstract class HandlerWrapper
    {
        public abstract Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken);

        public static HandlerWrapper Create(object handler, Type domainEventType)
        {
            Type wrapperType = WrapperTypeDictionary.GetOrAdd(
                domainEventType,
                et => typeof(HandlerWrapper<>).MakeGenericType(et));

            var wrapperInstance = Activator.CreateInstance(wrapperType, handler);
            return (HandlerWrapper) (wrapperInstance ?? throw new InvalidOperationException($"Failed to create handler wrapper for {domainEventType.Name}"));
        }
    }

    private sealed class HandlerWrapper<T>(object handler) : HandlerWrapper where T : IDomainEvent
    {
        private readonly IDomainEventHandler<T> _handler = (IDomainEventHandler<T>)handler;

        public override async Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            await _handler.Handle((T)domainEvent, cancellationToken);
        }
    }
}
