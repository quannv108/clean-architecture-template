---
type: Domain Slice
title: "Emails"
description: "The worked example slice: an entity that raises an event, delivered through the Outbox to a handler that sends."
resource: src/Domain/Emails
tags: [domain, email, example, domain-events]
status: stable
---

# Emails

A deliberately small slice that exercises the whole asynchronous machinery end to end. **It is a
demonstration** - delete it once you have a real slice of your own.

## The path it demonstrates

```
POST /api/v1/emails/test-send
  -> EmailMessage.Create(...)            entity factory returning Result<T>
  -> message.MarkAsSent()                behaviour method that Raise()s an event
  -> SaveChangesAsync()                  entity row + OutboxMessage row, one transaction
  -> OutboxMessageHostedService          picks it up later
  -> EmailSentDomainEventHandler         reacts
  -> IEmailSender                        actually sends
```

## Across the layers

| Layer | Files |
|---|---|
| Domain | [`EmailMessage`](email-message.md), [`EmailMessageStatus`](email-message-status.md), [`EmailErrors`](email-errors.md), [`EmailSentDomainEvent`](email-sent-domain-event.md) |
| Application | [`EmailSentDomainEventHandler`](../../../src/Application/ExampleDomainA/Events/EmailSentDomainEventHandler.cs), [`IEmailSender`](../../../src/Application/Abstractions/Communication/Email/IEmailSender.cs), [`EmailOptions`](../../../src/Application/Abstractions/Communication/Email/EmailOptions.cs) |
| Infrastructure | [`SesEmailSender`](../../../src/Infrastructure/Communication/Email/SesEmailSender.cs), [`DummyEmailSender`](../../../src/Infrastructure/Communication/Email/DummyEmailSender.cs), [`EmailMessageConfiguration`](../../../src/Infrastructure/Database/Configuration/Emails/Messages/EmailMessageConfiguration.cs) |
| Web.Api | [`SendTestEmail`](../../../src/Web.Api/Endpoints/Emails/SendTestEmail.cs), [email dev page](../../../src/Web.Api/Pages/Dev) |

## The lesson to keep

The command handler persists the *intent* and returns. Contacting a slow, failing third party happens in a
[domain event handler](../../../src/Application/Abstractions/Messaging/IQueryHandler.cs) afterwards, so provider trouble never becomes
the caller's problem and a retry is possible. Copy that shape for any external integration.

Note that the handler currently lives under `Application/ExampleDomainA/Events/` - a naming leftover worth
tidying when you rename the slice.

## Files in this slice

* [EmailMessage](email-message.md) - An email the system intended to send, with its status and the event it raises when sent.
* [EmailMessageStatus](email-message-status.md) - The lifecycle states of an email message, stored as a string.
* [EmailErrors](email-errors.md) - Domain error factories for email message failures.
* [EmailSentDomainEvent](email-sent-domain-event.md) - The template's worked domain event - an immutable positional record carrying only an id.

The handler is at
[EmailSentDomainEventHandler](../../../src/Application/ExampleDomainA/Events/EmailSentDomainEventHandler.cs);
the senders are in [`src/Infrastructure/Communication`](../../../src/Infrastructure/Communication).
