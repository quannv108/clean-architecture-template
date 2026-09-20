The worked example slice, showing entity, event, outbox and handler end to end. Start with the slice concept.

# Concepts

* [emails](emails.md) - The worked example slice: an entity that raises an event, delivered through the Outbox to a handler that sends.
* [EmailErrors](email-errors.md) - Domain error factories for email message failures.
* [EmailMessageStatus](email-message-status.md) - The lifecycle states of an email message, stored as a string.
* [EmailMessage](email-message.md) - An email the system intended to send, with its status and the event it raises when sent.
* [EmailSentDomainEvent](email-sent-domain-event.md) - The template's worked domain event - an immutable positional record carrying only an id.
