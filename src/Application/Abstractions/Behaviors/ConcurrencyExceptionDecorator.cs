using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Abstractions.Behaviors;

internal static class ConcurrencyExceptionDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            try
            {
                return await innerHandler.Handle(command, cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Result.Failure<TResponse>(ExtractConcurrencyError(ex));
            }
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            try
            {
                return await innerHandler.Handle(command, cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Result.Failure(ExtractConcurrencyError(ex));
            }
        }
    }

    private static Error ExtractConcurrencyError(DbUpdateConcurrencyException ex)
    {
        if (ex.Entries.Count == 0)
        {
            return Error.Conflict(
                "Concurrency.UpdateConflict",
                "The data was modified by another user. Please refresh and try again.");
        }

        // Try to extract entity information from the exception
        var entry = ex.Entries[0];
        var entityType = entry.Entity.GetType().Name;

        // Try to get the entity ID if it has one
        var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
        if (idProperty?.CurrentValue is Guid id)
        {
            return ConcurrencyErrors.UpdateConflict(entityType, id);
        }

        return ConcurrencyErrors.UpdateConflict(entityType);
    }
}
