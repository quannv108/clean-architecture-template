using Application.Abstractions.Communication.Sms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.PhoneNumbers;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using PhoneNumberE164 = Twilio.Types.PhoneNumber;

namespace Infrastructure.Communication.Sms;

internal sealed partial class TwilioSmsSender : ISmsSender
{
    private readonly ILogger<TwilioSmsSender> _logger;
    private readonly TwilioSmsOptions _options;

    public TwilioSmsSender(ILogger<TwilioSmsSender> logger, IOptions<SmsOptions> options)
    {
        _logger = logger;
        var smsOptions = options.Value;
        _options = smsOptions.Twilio;

        if (string.IsNullOrWhiteSpace(_options.AccountSid))
        {
            throw new ArgumentException("Twilio AccountSid must be configured (Sms:Twilio:AccountSid)");
        }

        if (string.IsNullOrWhiteSpace(_options.AuthToken))
        {
            throw new ArgumentException("Twilio AuthToken must be configured (Sms:Twilio:AuthToken)");
        }

        if (string.IsNullOrWhiteSpace(_options.MessagingServiceSid) && string.IsNullOrWhiteSpace(_options.FromNumber))
        {
            throw new ArgumentException(
                "Either Twilio MessagingServiceSid or FromNumber must be configured (Sms:Twilio:MessagingServiceSid or Sms:Twilio:FromNumber)");
        }

        TwilioClient.Init(_options.AccountSid, _options.AuthToken);
    }

    public async Task SendAsync(PhoneNumber phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure E.164 format with leading '+'
            var to = new PhoneNumberE164($"+{phoneNumber.PhoneNumberWithCallingCode()}");
            // Choose Messaging Service (preferred for Alphanumeric Sender ID) or fallback to FromNumber
            var useMessagingService = !string.IsNullOrWhiteSpace(_options.MessagingServiceSid);

            // Twilio SDK doesn't support passing CancellationToken directly; wrap with Task.Run if needed
            var msg = await MessageResource.CreateAsync(
                to: to,
                body: message,
                messagingServiceSid: useMessagingService ? _options.MessagingServiceSid : null,
                from: !useMessagingService && !string.IsNullOrWhiteSpace(_options.FromNumber)
                    ? new PhoneNumberE164(_options.FromNumber)
                    : null
            ).ConfigureAwait(false);

            LogSent(msg.Sid, to);
        }
        catch (Exception ex)
        {
            LogSendFailed(ex);
            throw new InvalidOperationException("Failed to send SMS via Twilio", ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Twilio SMS sent. SID: {Sid}, To: {To}")]
    private partial void LogSent(string sid, PhoneNumberE164 to);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to send SMS via Twilio")]
    private partial void LogSendFailed(Exception exception);
}
