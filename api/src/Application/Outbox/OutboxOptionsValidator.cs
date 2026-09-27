using Microsoft.Extensions.Options;

namespace Application.Outbox;

public sealed class OutboxOptionsValidator : IValidateOptions<OutboxOptions>
{
    public ValidateOptionsResult Validate(string? name, OutboxOptions options)
    {
        var errors = new List<string>();

        if (options.MinPollingIntervalMs > options.MaxPollingIntervalMs)
        {
            errors.Add(
                "Outbox MinPollingIntervalMs must be less than or equal to MaxPollingIntervalMs " +
                $"(Min: {options.MinPollingIntervalMs}, Max: {options.MaxPollingIntervalMs})");
        }

        if (errors.Count > 0)
        {
            return ValidateOptionsResult.Fail(errors);
        }

        return ValidateOptionsResult.Success;
    }
}
