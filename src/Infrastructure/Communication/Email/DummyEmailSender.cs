using Application.Abstractions.Communication.Email;
using Domain.Emails.Messages;
using SharedKernel;

namespace Infrastructure.Communication.Email;

internal class DummyEmailSender : IEmailSender
{
    public Task<Result> SendAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default)
    {
        // Log the email for debugging purposes
        Console.WriteLine($"Email sent to: {emailMessage.To}");
        Console.WriteLine($"Subject: {emailMessage.Subject}");
        Console.WriteLine($"Body: {emailMessage.Body}");

        return Task.FromResult(Result.Success());
    }
}
