using Microsoft.Extensions.Options;

namespace Infrastructure.Communication.Sms;

internal sealed class SmsOptionsValidator : IValidateOptions<SmsOptions>
{
    public ValidateOptionsResult Validate(string? name, SmsOptions options)
    {
        var errors = new List<string>();

        // If provider is Twilio, validate required Twilio settings
        if (string.Equals(options.Provider, "Twilio", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(options.Twilio.AccountSid))
            {
                errors.Add("Sms Twilio AccountSid is required when Provider is 'Twilio'");
            }

            if (string.IsNullOrWhiteSpace(options.Twilio.AuthToken))
            {
                errors.Add("Sms Twilio AuthToken is required when Provider is 'Twilio'");
            }

            // Either MessagingServiceSid or FromNumber must be provided
            if (string.IsNullOrWhiteSpace(options.Twilio.MessagingServiceSid) &&
                string.IsNullOrWhiteSpace(options.Twilio.FromNumber))
            {
                errors.Add("Sms Twilio MessagingServiceSid or FromNumber is required when Provider is 'Twilio'");
            }
        }

        if (errors.Count > 0)
        {
            return ValidateOptionsResult.Fail(errors);
        }

        return ValidateOptionsResult.Success;
    }
}
