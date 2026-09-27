using Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Abstractions.Behaviors;

internal static partial class LoggingDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        ILogger<CommandHandler<TCommand, TResponse>> logger)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            string commandName = typeof(TCommand).Name;

            LogProcessingCommand(logger, commandName);

            Result<TResponse> result = await innerHandler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                LogCompletedCommand(logger, commandName);
            }
            else
            {
                LogCommandFailed(logger, commandName, result.Error);
            }

            return result;
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        ILogger<CommandBaseHandler<TCommand>> logger)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            string commandName = typeof(TCommand).Name;

            LogProcessingCommand(logger, commandName);

            Result result = await innerHandler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                LogCompletedCommand(logger, commandName);
            }
            else
            {
                LogCommandFailed(logger, commandName, result.Error);
            }

            return result;
        }
    }

    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        ILogger<QueryHandler<TQuery, TResponse>> logger)
        : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            string queryName = typeof(TQuery).Name;

            LogProcessingQuery(logger, queryName);

            Result<TResponse> result = await innerHandler.Handle(query, cancellationToken);

            if (result.IsSuccess)
            {
                LogCompletedQuery(logger, queryName);
            }
            else
            {
                LogQueryFailed(logger, queryName, result.Error);
            }

            return result;
        }
    }

    internal sealed class DomainEventHandler<TDomainEvent>(
        IDomainEventHandler<TDomainEvent> innerHandler,
        ILogger<DomainEventHandler<TDomainEvent>> logger)
        : IDomainEventHandler<TDomainEvent>
        where TDomainEvent : IDomainEvent
    {
        public async Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            string eventName = typeof(TDomainEvent).Name;

            LogProcessingDomainEvent(logger, eventName);

            try
            {
                await innerHandler.Handle(domainEvent, cancellationToken);
                LogCompletedDomainEvent(logger, eventName);
            }
            catch (Exception ex)
            {
                LogDomainEventFailed(logger, ex, eventName);
                throw new InvalidOperationException(
                    $"Failed to process domain event {eventName}. See inner exception for details.", ex);
            }
        }
    }

    // The nested handlers are generic, so the generator cannot pick up their `logger` primary-ctor
    // parameter - ILogger is passed explicitly to static methods on this outer class.
    [LoggerMessage(Level = LogLevel.Information, Message = "Processing command {Command}")]
    private static partial void LogProcessingCommand(ILogger logger, string command);

    [LoggerMessage(Level = LogLevel.Information, Message = "Completed command {Command}")]
    private static partial void LogCompletedCommand(ILogger logger, string command);

    [LoggerMessage(Level = LogLevel.Error, Message = "Completed command {Command} with error: {Error}")]
    private static partial void LogCommandFailed(ILogger logger, string command, Error error);

    [LoggerMessage(Level = LogLevel.Information, Message = "Processing query {Query}")]
    private static partial void LogProcessingQuery(ILogger logger, string query);

    [LoggerMessage(Level = LogLevel.Information, Message = "Completed query {Query}")]
    private static partial void LogCompletedQuery(ILogger logger, string query);

    [LoggerMessage(Level = LogLevel.Error, Message = "Completed query {Query} with error: {Error}")]
    private static partial void LogQueryFailed(ILogger logger, string query, Error error);

    [LoggerMessage(Level = LogLevel.Information, Message = "Processing domain event {DomainEvent}")]
    private static partial void LogProcessingDomainEvent(ILogger logger, string domainEvent);

    [LoggerMessage(Level = LogLevel.Information, Message = "Completed domain event {DomainEvent}")]
    private static partial void LogCompletedDomainEvent(ILogger logger, string domainEvent);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error processing domain event {DomainEvent}")]
    private static partial void LogDomainEventFailed(ILogger logger, Exception exception, string domainEvent);
}
