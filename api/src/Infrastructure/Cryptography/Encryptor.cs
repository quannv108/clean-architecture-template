using System.Security.Cryptography;
using Application.Abstractions.Cryptography;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Infrastructure.Cryptography;

/// <summary>
/// AES 256 encryptor that uses a key from the configuration.
/// Supports multiple key versions for key rotation.
/// </summary>
internal sealed class Encryptor : IEncryptor
{
    private readonly int _currentKeyVersion;
    private readonly Dictionary<int, byte[]> _keys;

    public Encryptor(IOptions<EncryptionOptions> encryptionOptions)
    {
        var options = encryptionOptions.Value;
        _keys = new Dictionary<int, byte[]>();

        // Load current key version from configuration (defaults to 1 if not specified)
        _currentKeyVersion = options.CurrentVersion;

        // Load current key
        _keys[_currentKeyVersion] = Convert.FromBase64String(options.Key);

        // Load legacy keys if available
        foreach (var legacyKey in options.LegacyKeys)
        {
            var versionString = legacyKey.Key.TrimStart('V', 'v');
            if (int.TryParse(versionString, out var version) && version != _currentKeyVersion)
            {
                var keyValue = legacyKey.Value;
                if (!string.IsNullOrEmpty(keyValue))
                {
                    _keys[version] = Convert.FromBase64String(keyValue);
                }
            }
        }
    }

    public EncryptedString Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return new EncryptedString(plainText, _currentKeyVersion);
        }

        var encryptedValue = EncryptInternal(plainText, _keys[_currentKeyVersion]);
        return new EncryptedString(encryptedValue, _currentKeyVersion);
    }

    public string Decrypt(EncryptedString encryptedString)
    {
        ArgumentNullException.ThrowIfNull(encryptedString);

        if (string.IsNullOrEmpty(encryptedString.Value))
        {
            return encryptedString.Value;
        }

        // Get the key for the version stored in the encrypted string
        if (!_keys.TryGetValue(encryptedString.KeyVersion, out var key))
        {
            throw new InvalidOperationException(
                $"Encryption key version {encryptedString.KeyVersion} not found. Available versions: {string.Join(", ", _keys.Keys)}");
        }

        return DecryptInternal(encryptedString.Value, key);
    }

    private static string EncryptInternal(string plainText, byte[] key)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return plainText;
        }

        using var aes = Aes.Create(); // default KeySize is 256 bits
        aes.Key = key;
        aes.GenerateIV(); // Generate a new IV for each encryption

        var encryptor = aes.CreateEncryptor();

        using var msEncrypt = new MemoryStream();
        // Prepend the IV to the encrypted data
        msEncrypt.Write(aes.IV, 0, aes.IV.Length);

        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    private static string DecryptInternal(string cipherText, byte[] key)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return cipherText;
        }

        var buffer = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create(); // default KeySize is 256 bits
        aes.Key = key;

        // Extract the IV from the beginning of the encrypted data
        var iv = new byte[aes.BlockSize / 8];
        Array.Copy(buffer, 0, iv, 0, iv.Length);
        aes.IV = iv;

        var decryptor = aes.CreateDecryptor();

        using var msDecrypt = new MemoryStream(buffer, iv.Length, buffer.Length - iv.Length);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        return srDecrypt.ReadToEnd();
    }
}
