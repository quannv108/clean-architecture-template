---
type: Term
title: "Encryption at Rest"
description: "Encrypting stored data so the database file alone does not reveal it."
tags: [security, encryption]
status: stable
---

# Encryption at Rest

Here it is field-level: declare a property as [`EncryptedString`](../../src/SharedKernel/EncryptedString.cs) and it
is encrypted with AES-256 on write and decrypted on read.

Field-level encryption protects against database compromise, but an encrypted column cannot be indexed or
compared server-side - so encrypt what is sensitive, not everything, and keep a
[hash](hashing.md) alongside anything you must look up by equality.

See [Field Encryption and Key Rotation](../engineering/patterns/encryption.md).
