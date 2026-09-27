using Application.Abstractions.Behaviors;
using Application.Abstractions.Messaging;
using Application.Outbox;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // NOTE: when usings Scruttor to register implementation,
        // we should careful and avoid double-registration by decorations implementation
        // else we will have some strange behavior like a domain event is handled twice, etc...

        // Commands and Queries
        services.Scan(scan => scan.FromAssembliesOf(typeof(DependencyInjection))
            .AddClasses(classes => classes
                    .AssignableTo(typeof(IQueryHandler<,>))
                    .Where(x => !x.IsNested), // ignore decorators implementations
                publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(classes => classes
                    .AssignableTo(typeof(ICommandHandler<>))
                    .Where(x => !x.IsNested), // ignore decorators implementations
                publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(classes => classes
                    .AssignableTo(typeof(ICommandHandler<,>))
                    .Where(x => !x.IsNested), // ignore decorators implementation
                publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Domain events
        services.Scan(scan => scan.FromAssembliesOf(typeof(DependencyInjection))
            .AddClasses(classes => classes
                    .AssignableTo(typeof(IDomainEventHandler<>))
                    .Where(x => !x.IsNested), // avoid decorator
                publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Decorators mean that the actual command/query handlers are wrapped with additional functionality, such as validation or logging.
        // Without decorators, the handlers would be executed directly without any additional processing.
        // So, decorators are a way to extend the functionality of the handlers without modifying their code.
        services.Decorate(typeof(ICommandHandler<,>), typeof(OpenTelemetryInstrumentDecorator.CommandHandler<,>));
        services.Decorate(typeof(IQueryHandler<,>), typeof(OpenTelemetryInstrumentDecorator.QueryHandler<,>));
        services.Decorate(typeof(ICommandHandler<>), typeof(OpenTelemetryInstrumentDecorator.CommandBaseHandler<>));
        services.Decorate(typeof(IDomainEventHandler<>), typeof(OpenTelemetryInstrumentDecorator.DomainEventHandler<>));

        services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator.CommandHandler<,>));
        services.Decorate(typeof(ICommandHandler<>), typeof(ValidationDecorator.CommandBaseHandler<>));

        services.Decorate(typeof(ICommandHandler<,>), typeof(ConcurrencyExceptionDecorator.CommandHandler<,>));
        services.Decorate(typeof(ICommandHandler<>), typeof(ConcurrencyExceptionDecorator.CommandBaseHandler<>));

        services.Decorate(typeof(IQueryHandler<,>), typeof(LoggingDecorator.QueryHandler<,>));
        services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandler<,>));
        services.Decorate(typeof(ICommandHandler<>), typeof(LoggingDecorator.CommandBaseHandler<>));
        services.Decorate(typeof(IDomainEventHandler<>), typeof(LoggingDecorator.DomainEventHandler<>));

        // outbox processing
        services.AddScoped<IOutboxMessageProcessor, OutboxMessageProcessor>();

        return services;
    }
}
