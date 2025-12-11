using Domain.Emails.Messages;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.ExampleDomainA.Events;

internal class EmailSentDomainEventHandler(ILogger<EmailSentDomainEventHandler> logger)
    : IDomainEventHandler<EmailSentDomainEvent>
{
    public Task Handle(EmailSentDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Email {Id} sent", domainEvent.EmailMessageId);
        return Task.CompletedTask;
    }
}
