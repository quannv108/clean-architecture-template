using System.Collections.Concurrent;
using System.Diagnostics;
using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Abstractions.Behaviors;

internal static class OpenTelemetryInstrumentDecorator
{
    private static readonly ConcurrentDictionary<Type, ActivitySource> ActivitySources = new();

    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        IUserContext userContext)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var activitySource = ActivitySources.GetOrAdd(typeof(TCommand),
                _ => new ActivitySource($"Application.{typeof(TCommand).Name}"));

            using var activity =
                activitySource.StartActivity($"Execute {typeof(TCommand).Name}", ActivityKind.Internal);

            if (activity is not null)
            {
                // Set meaningful tags
                activity.SetTag("operation.name", typeof(TCommand).Name);

                if (userContext.UserId.HasValue)
                {
                    activity.SetTag("user.id", userContext.UserId.Value.ToString());
                }

                if (userContext.TenantId.HasValue)
                {
                    activity.SetTag("tenant.id", userContext.TenantId.Value.ToString());
                }

                // Propagate baggage from current activity
                if (Activity.Current is not null)
                {
                    foreach (var baggageItem in Activity.Current.Baggage)
                    {
                        activity.SetBaggage(baggageItem.Key, baggageItem.Value);
                    }
                }
            }

            try
            {
                var result = await innerHandler.Handle(command, cancellationToken);

                if (activity is not null)
                {
                    if (result.IsFailure)
                    {
                        activity.SetStatus(ActivityStatusCode.Error, result.Error.Description);
                        activity.SetTag("error", true);
                        activity.SetTag("error.type", result.Error.Type.ToString());
                    }
                    else
                    {
                        activity.SetStatus(ActivityStatusCode.Ok);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("error", true);
                activity?.SetTag("error.type", "Exception");
                throw;
            }
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        IUserContext userContext)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var activitySource = ActivitySources.GetOrAdd(typeof(TCommand),
                _ => new ActivitySource($"Application.{typeof(TCommand).Name}"));

            using var activity =
                activitySource.StartActivity($"Execute {typeof(TCommand).Name}", ActivityKind.Internal);

            if (activity is not null)
            {
                // Set meaningful tags
                activity.SetTag("operation.type", "command");
                activity.SetTag("operation.name", typeof(TCommand).Name);

                if (userContext.UserId.HasValue)
                {
                    activity.SetTag("user.id", userContext.UserId.Value.ToString());
                }

                if (userContext.TenantId.HasValue)
                {
                    activity.SetTag("tenant.id", userContext.TenantId.Value.ToString());
                }

                // Propagate baggage from current activity
                if (Activity.Current is not null)
                {
                    foreach (var baggageItem in Activity.Current.Baggage)
                    {
                        activity.SetBaggage(baggageItem.Key, baggageItem.Value);
                    }
                }
            }

            try
            {
                var result = await innerHandler.Handle(command, cancellationToken);

                if (activity is not null)
                {
                    if (result.IsFailure)
                    {
                        activity.SetStatus(ActivityStatusCode.Error, result.Error.Description);
                        activity.SetTag("error", true);
                        activity.SetTag("error.type", result.Error.Type.ToString());
                    }
                    else
                    {
                        activity.SetStatus(ActivityStatusCode.Ok);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("error", true);
                activity?.SetTag("error.type", "Exception");
                throw;
            }
        }
    }

    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        IUserContext userContext)
        : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            var activitySource = ActivitySources.GetOrAdd(typeof(TQuery),
                _ => new ActivitySource($"Application.{typeof(TQuery).Name}"));

            using var activity = activitySource.StartActivity($"Execute {typeof(TQuery).Name}", ActivityKind.Internal);

            if (activity is not null)
            {
                // Set meaningful tags
                activity.SetTag("operation.type", "query");
                activity.SetTag("operation.name", typeof(TQuery).Name);

                if (userContext.UserId.HasValue)
                {
                    activity.SetTag("user.id", userContext.UserId.Value.ToString());
                }

                if (userContext.TenantId.HasValue)
                {
                    activity.SetTag("tenant.id", userContext.TenantId.Value.ToString());
                }

                // Propagate baggage from current activity
                if (Activity.Current is not null)
                {
                    foreach (var baggageItem in Activity.Current.Baggage)
                    {
                        activity.SetBaggage(baggageItem.Key, baggageItem.Value);
                    }
                }
            }

            try
            {
                var result = await innerHandler.Handle(query, cancellationToken);

                if (activity is not null)
                {
                    if (result.IsFailure)
                    {
                        activity.SetStatus(ActivityStatusCode.Error, result.Error.Description);
                        activity.SetTag("error", true);
                        activity.SetTag("error.type", result.Error.Type.ToString());
                    }
                    else
                    {
                        activity.SetStatus(ActivityStatusCode.Ok);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("error", true);
                activity?.SetTag("error.type", "Exception");
                throw;
            }
        }
    }

    internal sealed class DomainEventHandler<TDomainEvent>(
        IDomainEventHandler<TDomainEvent> innerHandler,
        IUserContext userContext)
        : IDomainEventHandler<TDomainEvent>
        where TDomainEvent : IDomainEvent
    {
        public async Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            var activitySource = ActivitySources.GetOrAdd(typeof(TDomainEvent),
                _ => new ActivitySource($"Application.{typeof(TDomainEvent).Name}"));

            using var activity =
                activitySource.StartActivity($"Handle {typeof(TDomainEvent).Name}", ActivityKind.Internal);

            if (activity is not null)
            {
                // Set meaningful tags
                activity.SetTag("operation.type", "domain_event");
                activity.SetTag("operation.name", typeof(TDomainEvent).Name);

                if (userContext.UserId.HasValue)
                {
                    activity.SetTag("user.id", userContext.UserId.Value.ToString());
                }

                if (userContext.TenantId.HasValue)
                {
                    activity.SetTag("tenant.id", userContext.TenantId.Value.ToString());
                }

                // Propagate baggage from current activity
                if (Activity.Current is not null)
                {
                    foreach (var baggageItem in Activity.Current.Baggage)
                    {
                        activity.SetBaggage(baggageItem.Key, baggageItem.Value);
                    }
                }
            }

            try
            {
                await innerHandler.Handle(domainEvent, cancellationToken);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("error", true);
                activity?.SetTag("error.type", "Exception");
                throw;
            }
        }
    }
}
