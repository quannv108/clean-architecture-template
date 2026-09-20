---
type: Term
title: "Hashing"
description: "A one-way transformation used where a value must be checked but never read back."
tags: [security, cryptography]
status: stable
---

# Hashing

[`IHasher`](../../src/Application/Abstractions/Cryptography/IHasher.cs). Not interchangeable with encryption: a hash has no inverse.

Use it for passwords and tokens, and as a deterministic lookup key beside an
[`EncryptedString`](../../src/SharedKernel/EncryptedString.cs) column you need to search by equality - since the
encrypted column itself cannot be compared server-side.
