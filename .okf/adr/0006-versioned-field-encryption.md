---
type: ADR
title: "ADR 0006: Versioned AES-256 field encryption via EncryptedString"
description: "Encrypt sensitive columns through a value type whose stored format carries its key version, so keys can rotate without a data migration."
tags: [adr, encryption, security, key-rotation]
status: stable
generated:
  by: anthropic/claude-opus-5
  at: 2026-09-19T00:00:00Z
---

# ADR 0006: Versioned AES-256 field encryption via EncryptedString

**Status:** Accepted - reconstructed on 2026-09-19 from the codebase and its previous `docs/`
tree. The decision was already in force; this record was written afterwards, so the context and
alternatives are inferred. Correct them if you were there.

## Context

Sensitive fields need encryption at rest beyond what disk encryption provides. The hard part is not
encrypting - it is rotating a key afterwards. If the ciphertext does not record which key produced it,
rotation means decrypting and re-encrypting every row before the new key can be used, in one migration, with
downtime.

## Decision

A [`EncryptedString`](../../src/SharedKernel/EncryptedString.cs) property type stored as
`{KeyVersion}:{CipherText}`, converted transparently by
[`EncryptedStringConverter`](../../src/Infrastructure/Database/Converters/EncryptedStringConverter.cs) calling
[`IEncryptor`](../../src/Application/Abstractions/Cryptography/IEncryptor.cs).
[`EncryptionOptions`](../../src/Application/Abstractions/Cryptography/EncryptionOptions.cs) holds the current version, its key, and
`LegacyKeys` for previous versions.

Rotation is a configuration change: increment `CurrentVersion`, add the new key, move the old one to
`LegacyKeys`. New writes use the new key; old rows keep decrypting with theirs. Re-encryption becomes an
optional background task rather than a prerequisite.

## Consequences

**Good.** Rotation with no downtime and no data migration. Declaring a property as `EncryptedString` is the
entire developer-facing API. Mixed-version data is normal rather than an error state.

**Costly.** Encrypted columns cannot be indexed, sorted, or compared server-side - equality lookup needs a
separate deterministic hash ([`IHasher`](../../src/Application/Abstractions/Cryptography/IHasher.cs)) alongside. Ciphertext is larger than
plaintext, so column sizes need headroom. Key management moves outside the application, and **removing a
legacy key before every row is re-encrypted makes those rows permanently unreadable** - the failure surfaces
far from its cause, as "Encryption key version X not found".

Procedure: [Field Encryption and Key Rotation](../engineering/patterns/encryption.md).
