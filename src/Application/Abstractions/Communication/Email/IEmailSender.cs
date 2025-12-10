using Domain.Emails.Messages;
using SharedKernel;

namespace Application.Abstractions.Communication.Email;

public interface IEmailSender
{
    Task<Result> SendAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default);
}
