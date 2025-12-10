# Encryption and Key Management

This document explains how encryption works in the application, how to manage encryption keys, and how to perform key rotation.

## Overview

The application uses AES-256 encryption for sensitive data with support for multiple key versions, enabling seamless key rotation without downtime or data loss.

## Architecture Components

- **`EncryptedString`** (`SharedKernel`): Value object storing encrypted data with version info (format: `{KeyVersion}:{EncryptedValue}`)
- **`IEncryptor`** (`Application.Abstractions.Cryptography`): Encryption service interface with versioned methods
- **`Encryptor`** (`Infrastructure.Cryptography`): AES-256 implementation supporting multiple key versions
- **`EncryptedStringConverter`** (`Infrastructure.Database.Converters`): EF Core value converter (registered globally)

## Configuration

### Basic Configuration (Single Key)

```json
{
  "Encryption": {
    "CurrentVersion": 1,
    "Key": "your-base64-encoded-256-bit-key-here"
  }
}
```

**Generate a key:** `openssl rand -base64 32`

### Configuration with Legacy Keys (After Rotation)

```json
{
  "Encryption": {
    "CurrentVersion": 2,
    "Key": "new-base64-encoded-key-for-v2",
    "LegacyKeys": {
      "V1": "old-base64-encoded-key-for-v1"
    }
  }
}
```

## Usage

### Define Encrypted Properties in Entities

Properties of type `EncryptedString` are automatically encrypted/decrypted by EF Core. No additional configuration needed.

### Encrypt Data in Command Handlers

Inject `IEncryptor` and use `EncryptWithVersion()` method to encrypt sensitive data before saving.

### Decrypt Data in Query Handlers

Inject `IEncryptor` and use `DecryptWithVersion()` method to decrypt data when reading. The version is automatically detected from the stored value.

## Key Rotation

Key rotation enables changing encryption keys without downtime or data loss.

### Process

**Step 1: Generate New Key**
```bash
openssl rand -base64 32
```

**Step 2: Update Configuration**
```json
{
  "Encryption": {
    "CurrentVersion": 2,
    "Key": "NEW-BASE64-KEY",
    "LegacyKeys": {
      "V1": "OLD-BASE64-KEY"
    }
  }
}
```
- Increment `CurrentVersion`
- Move old key to `LegacyKeys.V{OldVersion}`
- Set new key as `Key`

**Step 3: Deploy Configuration**
- New encryptions automatically use version 2
- Old data (version 1) remains decryptable via legacy key
- No database migration required

**Step 4: Re-encrypt Old Data (Optional)**

Create a background job that reads old encrypted data, decrypts with legacy key, and re-encrypts with current key version.

**Step 5: Remove Legacy Keys**

After all data is re-encrypted, remove the `LegacyKeys` section from configuration.

## Database Storage

Data is stored in the format: `{KeyVersion}:{EncryptedValue}` (e.g., `2:base64encrypteddata`)

Use standard `varchar`/`nvarchar` columns with appropriate max length (e.g., 500). The converter is applied automatically.

## Security Best Practices

### Key Storage

**DO:**
- Store keys in secure secret management (Azure Key Vault, AWS Secrets Manager)
- Use environment variables for local development
- Rotate keys regularly
- Restrict access to key configuration

**DON'T:**
- Commit keys to source control
- Share keys via email/chat
- Use same key across environments

### Key Rotation Schedule

- **High-security data** (SSN, payment info): Every 6-12 months
- **Medium-security data** (PII): Every 12-24 months
- **Compromised keys**: Immediately

### Key Backup

- Backup encryption keys securely before rotation
- Store backups in different secure location
- Document key version history
- Test key recovery procedures

## Troubleshooting

### Error: "Encryption key version X not found"

**Solution:** Add missing key version to `LegacyKeys` configuration.

### Error: "Encryption key not configured"

**Solution:** Add the required `Encryption:Key` configuration.

### Invalid Decryption

**Solution:**
- Verify correct key version is configured
- Check data integrity in database
- Ensure format is `{version}:{encryptedValue}`

## Migration from Non-Versioned Encryption

If you have existing encrypted data without version support:

1. Add version 1 configuration with your existing key
2. Create migration to prepend "1:" to existing encrypted values in database
3. Start using `EncryptWithVersion()` for new encryptions
