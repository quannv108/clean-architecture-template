---
type: Pattern
title: "Field Encryption and Key Rotation"
description: "Encrypt a column by declaring EncryptedString, and rotate keys by configuration without a data migration."
resource: src/Infrastructure/Cryptography
tags: [encryption, security, key-rotation, aes]
status: stable
---

# Field Encryption and Key Rotation

## Encrypting a field

Declare the property as [`EncryptedString`](../../../src/SharedKernel/EncryptedString.cs). That is the whole change -
[`EncryptedStringConverter`](../../../src/Infrastructure/Database/Converters/EncryptedStringConverter.cs) is registered globally and calls
[`IEncryptor`](../../../src/Application/Abstractions/Cryptography/IEncryptor.cs) in both directions.

Use a `varchar`/`nvarchar` column with headroom for the `{version}:` prefix and base64 expansion; 500 is a
reasonable default.

The stored value is `{KeyVersion}:{base64cipher}` (AES-256), so each row records which key encrypted it and
stays readable after a rotation.

| Failure | Cause |
|---|---|
| `Encryption key version X not found` | `LegacyKeys.V{X}` removed before every row was re-encrypted |
| `Encryption key not configured` | `Encryption:Key` missing for the current version |
| Garbled output | The stored value lost its `{version}:` prefix |

## Configuration

`Encryption:CurrentVersion` (the version new values use), `Encryption:Key` (its base64 256-bit key, from
`openssl rand -base64 32`) and `Encryption:LegacyKeys:V{n}` for every previous version still present in the
database. **Never commit keys** - user secrets or environment variables locally, a secret manager when
deployed.

## Rotating a key

1. **Generate** a new key: `openssl rand -base64 32`.
2. **Update configuration** - increment `CurrentVersion`, set the new key as `Key`, move the old one to
   `LegacyKeys.V{OldVersion}`:

   ```json
   {
     "Encryption": {
       "CurrentVersion": 2,
       "Key": "NEW-BASE64-KEY",
       "LegacyKeys": { "V1": "OLD-BASE64-KEY" }
     }
   }
   ```

3. **Deploy.** New writes use version 2; version 1 rows keep decrypting with the legacy key. No migration,
   no downtime.
4. **Re-encrypt old rows** (optional) with a background job that reads, decrypts and saves - the save
   re-encrypts at the current version.
5. **Remove the legacy key** only once every row is re-encrypted.

**Step 5 is the dangerous one.** Removing a legacy key before all its rows are re-encrypted makes them
permanently unreadable, and the failure ("Encryption key version X not found") appears whenever someone next
reads one - possibly months later. Verify before removing.

## Limits to design around

* Encrypted columns cannot be indexed, sorted or compared server-side. For equality lookup, store a
  deterministic hash ([`IHasher`](../../../src/Application/Abstractions/Cryptography/IHasher.cs)) in a second column and query that.
* Rotation cadence: 6-12 months for high-sensitivity data, 12-24 for PII, **immediately** if a key is
  compromised.
* Back keys up before rotating, store backups separately, and test recovery.

## Migrating existing unversioned data

Add version 1 configuration with the existing key, run a migration that prepends `1:` to existing values,
then rotate normally.

Decision: [ADR 0006: Versioned AES-256 field encryption via EncryptedString](../../adr/0006-versioned-field-encryption.md).
