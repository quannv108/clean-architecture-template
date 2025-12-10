using Application.Abstractions.Communication.Sms;
using Microsoft.Extensions.Logging;
using SharedKernel.Common;

namespace Infrastructure.Communication.Sms;

internal class DummySmsSender(ILogger<DummySmsSender> logger) : ISmsSender
{
    public Task SendAsync(PhoneNumber phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        // This is a dummy implementation that does nothing.
        // In a real application, this would send an SMS using an external service.
        logger.LogInformation("{SmsSender} has been use to send message {Message}", nameof(DummySmsSender), message);
        return Task.CompletedTask;
    }
}
