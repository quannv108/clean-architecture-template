using Application.Abstractions.Communication.Sms;
using Microsoft.Extensions.Logging;
using SharedKernel.PhoneNumbers;

namespace Infrastructure.Communication.Sms;

internal partial class DummySmsSender(ILogger<DummySmsSender> logger) : ISmsSender
{
    public Task SendAsync(PhoneNumber phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        // This is a dummy implementation that does nothing.
        // In a real application, this would send an SMS using an external service.
        LogSent(nameof(DummySmsSender), message);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "{SmsSender} has been use to send message {Message}")]
    private partial void LogSent(string smsSender, string message);
}
