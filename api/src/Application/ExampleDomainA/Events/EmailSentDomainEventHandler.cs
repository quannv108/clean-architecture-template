using Domain.Emails;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.ExampleDomainA.Events;

internal sealed partial class EmailSentDomainEventHandler(ILogger<EmailSentDomainEventHandler> logger)
    : IDomainEventHandler<EmailSentDomainEvent>
{
    public Task Handle(EmailSentDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        LogEmailSent(domainEvent.EmailMessageId);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Email {Id} sent")]
    private partial void LogEmailSent(Guid id);
}
