using System.ComponentModel.DataAnnotations;
using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Abstractions.Behaviors;

internal static class ValidationDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var validationFailures = ValidateRequest(command);

            if (validationFailures.Count == 0)
            {
                return await innerHandler.Handle(command, cancellationToken);
            }

            return Result.Failure<TResponse>(CreateValidationError(validationFailures));
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var validationFailures = ValidateRequest(command);

            if (validationFailures.Count == 0)
            {
                return await innerHandler.Handle(command, cancellationToken);
            }

            return Result.Failure(CreateValidationError(validationFailures));
        }
    }

    private static List<ValidationResult> ValidateRequest<TCommand>(TCommand request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request);

        // Validate using data annotations
        Validator.TryValidateObject(request, validationContext, validationResults, true);

        // Validate using IValidatableObject if implemented
        if (request is IValidatableObject validatableObject)
        {
            var objectValidationResults = validatableObject.Validate(validationContext);
            validationResults.AddRange(objectValidationResults);
        }

        return validationResults;
    }

    private static ValidationError CreateValidationError(List<ValidationResult> validationResults) =>
        new(validationResults.Select(vr =>
            Error.Problem(
                vr.MemberNames.FirstOrDefault() ?? "General",
                vr.ErrorMessage ?? "Validation failed"
            )).ToArray());
}
