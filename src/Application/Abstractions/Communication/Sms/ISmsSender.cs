using SharedKernel.Common;

namespace Application.Abstractions.Communication.Sms;

public interface ISmsSender
{
    Task SendAsync(PhoneNumber phoneNumber, string message, CancellationToken cancellationToken = default);
}
