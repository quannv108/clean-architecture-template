namespace Infrastructure.Communication.Sms;

internal sealed class SmsOptions
{
    // Set Sms:Provider to "Twilio" to enable Twilio sender; defaults to Dummy
    public string? Provider { get; set; }

    public TwilioSmsOptions Twilio { get; set; } = new();
}

internal sealed class TwilioSmsOptions
{
    // Required
    public string? AccountSid { get; set; }
    public string? AuthToken { get; set; }

    // Preferred: Messaging Service configured with Alphanumeric Sender ID
    // If provided, messages will be sent using this service instead of a specific phone number.
    public string? MessagingServiceSid { get; set; } // e.g. MGXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

    // Fallback: send from a specific phone number (E.164), used only if MessagingServiceSid is not set
    public string? FromNumber { get; set; } // E.164 format, e.g. +15551234567
}
