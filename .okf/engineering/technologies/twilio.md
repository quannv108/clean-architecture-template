---
type: Technology
title: "Twilio"
description: "The SMS provider behind TwilioSmsSender."
resource: https://www.twilio.com
tags: [technology, sms, external-service]
status: stable
---

# Twilio

Used by [`TwilioSmsSender`](../../../src/Infrastructure/Communication/Sms/TwilioSmsSender.cs), configured through
[`SmsOptions`](../../../src/Application/Abstractions/Communication/Sms/SmsOptions.cs). Locally,
[`DummySmsSender`](../../../src/Infrastructure/Communication/Sms/DummySmsSender.cs) stands in.

Numbers should be validated [`PhoneNumber`](../../../src/SharedKernel/PhoneNumbers/PhoneNumber.cs) values before reaching the sender.
SMS costs money per message, is rate limited, and delivery is not guaranteed - so send from a
[domain event handler](../patterns/domain-event.md), make it idempotent, and remember that
[outbox delivery is at-least-once](../patterns/outbox-pattern.md): a non-idempotent handler will eventually
send twice and bill twice.
