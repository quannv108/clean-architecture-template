#pragma warning disable CA1873
using Domain.Emails;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.ExampleDomainA.Events;

internal sealed class EmailSentDomainEventHandler(ILogger<EmailSentDomainEventHandler> logger)
    : IDomainEventHandler<EmailSentDomainEvent>
{
    public Task Handle(EmailSentDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Email {Id} sent", domainEvent.EmailMessageId);
        return Task.CompletedTask;
    }
}
