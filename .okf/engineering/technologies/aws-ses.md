---
type: Technology
title: "Amazon SES"
description: "The email provider behind SesEmailSender."
resource: https://aws.amazon.com/ses/
tags: [technology, email, aws, external-service]
status: stable
---

# Amazon SES

Used by [`SesEmailSender`](../../../src/Infrastructure/Communication/Email/SesEmailSender.cs), configured through
[`EmailOptions`](../../../src/Application/Abstractions/Communication/Email/EmailOptions.cs). Locally,
[`DummyEmailSender`](../../../src/Infrastructure/Communication/Email/DummyEmailSender.cs) stands in so nothing external is needed.

Operational realities to design for: sender identities and domains must be verified, new accounts are in a
sandbox that only sends to verified addresses, and sending is rate limited.

All of which is why mail is sent from a [domain event handler](../patterns/domain-event.md) rather than
inside a command - the provider's latency and failures stay out of the request, and a retry is possible.
