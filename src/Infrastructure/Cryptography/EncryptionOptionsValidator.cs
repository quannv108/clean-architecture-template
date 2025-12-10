using Microsoft.Extensions.Options;

namespace Infrastructure.Cryptography;

internal sealed class EncryptionOptionsValidator : IValidateOptions<EncryptionOptions>
{
    public ValidateOptionsResult Validate(string? name, EncryptionOptions options)
    {
        var errors = new List<string>();

        // Validate that the key is a valid base64 string
        if (!string.IsNullOrWhiteSpace(options.Key))
        {
            try
            {
                var keyBytes = Convert.FromBase64String(options.Key);

                // AES-256 requires a 32-byte (256-bit) key
                if (keyBytes.Length != 32)
                {
                    errors.Add(
                        $"Encryption Key must be a base64-encoded 256-bit (32-byte) key. Current key decodes to {keyBytes.Length} bytes.");
                }
            }
            catch (FormatException)
            {
                errors.Add("Encryption Key must be a valid base64-encoded string");
            }
        }

        // Validate legacy keys if present
        foreach (var legacyKey in options.LegacyKeys)
        {
            if (string.IsNullOrWhiteSpace(legacyKey.Value))
            {
                continue;
            }

            try
            {
                var keyBytes = Convert.FromBase64String(legacyKey.Value);
                if (keyBytes.Length != 32)
                {
                    errors.Add(
                        $"Encryption LegacyKey '{legacyKey.Key}' must be a base64-encoded 256-bit (32-byte) key. Current key decodes to {keyBytes.Length} bytes.");
                }
            }
            catch (FormatException)
            {
                errors.Add($"Encryption LegacyKey '{legacyKey.Key}' must be a valid base64-encoded string");
            }
        }

        if (errors.Count > 0)
        {
            return ValidateOptionsResult.Fail(errors);
        }

        return ValidateOptionsResult.Success;
    }
}
