using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Cryptography;

internal sealed class EncryptionOptions
{
    [Required(ErrorMessage = "Encryption Key is required")]
    [MinLength(32, ErrorMessage = "Encryption Key must be at least 32 characters (base64-encoded 256-bit key)")]
    public string Key { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Encryption CurrentVersion must be a positive integer")]
    public int CurrentVersion { get; set; } = 1;

    public Dictionary<string, string> LegacyKeys { get; set; } = new();
}
