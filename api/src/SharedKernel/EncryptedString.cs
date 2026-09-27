namespace SharedKernel;

/// <summary>
/// Represents an encrypted string with versioning support for key rotation.
/// Format: {KeyVersion}:{EncryptedValue}
/// </summary>
public sealed class EncryptedString
{
    public string Value { get; }
    public int KeyVersion { get; }

    /// <summary>
    /// Creates a new instance with explicit values.
    /// </summary>
    public EncryptedString(string value, int keyVersion)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        KeyVersion = keyVersion;
    }

    /// <summary>
    /// Parses a combined string in format "{KeyVersion}:{EncryptedValue}".
    /// </summary>
    public EncryptedString(string combinedValue)
    {
        if (string.IsNullOrEmpty(combinedValue))
        {
            throw new ArgumentException("Combined value cannot be null or empty.", nameof(combinedValue));
        }

        var parts = combinedValue.Split(':', 2);
        if (parts.Length != 2)
        {
            // fallback to 1st version
            KeyVersion = 1;
            Value = combinedValue;
            return;
        }

        if (!int.TryParse(parts[0], out var version))
        {
            throw new ArgumentException("KeyVersion must be a valid integer.", nameof(combinedValue));
        }

        KeyVersion = version;
        Value = parts[1];
    }

    /// <summary>
    /// Combines KeyVersion and Value into format "{KeyVersion}:{EncryptedValue}".
    /// </summary>
    public override string ToString()
    {
        return $"{KeyVersion}:{Value}";
    }
}
