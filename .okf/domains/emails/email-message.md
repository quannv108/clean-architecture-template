---
type: Entity
title: "EmailMessage"
description: "An email the system intended to send, with its status and the event it raises when sent."
resource: src/Domain/Emails/EmailMessage.cs
tags: [domain, email, entity]
status: stable
---

# EmailMessage

Persisting the message rather than just calling a provider is the point: "we meant to send this" is a
durable fact that survives a provider outage, a restart and a retry.

Three things worth copying from `MarkAsSent` method: the guard makes it idempotent, the state change happens on the
entity rather than in a handler, and the event carries only the id.

Created through a `Create(...)` factory returning [`Result<EmailMessage>`](../../../src/SharedKernel/Result.cs); failures
come from [`EmailErrors`](email-errors.md). Status values:
[`EmailMessageStatus`](email-message-status.md). Mapping:
[`EmailMessageConfiguration`](../../../src/Infrastructure/Database/Configuration/Emails/Messages/EmailMessageConfiguration.cs).
