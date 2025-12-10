using SharedKernel;

namespace Domain.Emails.Messages;

public record EmailSentDomainEvent(Guid EmailMessageId) : IDomainEvent;
