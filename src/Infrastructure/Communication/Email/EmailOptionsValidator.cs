using Microsoft.Extensions.Options;

namespace Infrastructure.Communication.Email;

internal sealed class EmailOptionsValidator : IValidateOptions<EmailOptions>
{
    public ValidateOptionsResult Validate(string? name, EmailOptions options)
    {
        var errors = new List<string>();

        // If provider is SES, validate required SES settings
        if (string.Equals(options.Provider, "SES", StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrWhiteSpace(options.Ses.Region))
        {
            errors.Add("Email SES Region is required when Provider is 'SES'");
        }

        // Note: AccessKeyId and SecretAccessKey are optional when using AWS SDK default credentials
        // (environment variables, IAM role, etc.), so we don't validate them here

        if (errors.Count > 0)
        {
            return ValidateOptionsResult.Fail(errors);
        }

        return ValidateOptionsResult.Success;
    }
}
