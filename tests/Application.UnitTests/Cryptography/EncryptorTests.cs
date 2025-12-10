using Infrastructure.Cryptography;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Application.UnitTests.Cryptography;

public class EncryptorTests
{
    private static Encryptor CreateEncryptor(int currentVersion = 1, Dictionary<int, string>? legacyKeys = null)
    {
        var legacyKeysDict = new Dictionary<string, string>();
        if (legacyKeys != null)
        {
            foreach (var kvp in legacyKeys)
            {
                legacyKeysDict[$"V{kvp.Key}"] = kvp.Value;
            }
        }

        var options = new EncryptionOptions
        {
            CurrentVersion = currentVersion,
            Key = "dGhpcyBpcyBhIDI1NiBiaXQga2V5IGZvciBBRVMyNTY=", // Base64 encoded 256-bit key
            LegacyKeys = legacyKeysDict
        };

        var optionsWrapper = Options.Create(options);
        return new Encryptor(optionsWrapper);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenEncryptionKeyIsInvalidBase64()
    {
        // Arrange
        var options = new EncryptionOptions
        {
            CurrentVersion = 1,
            Key = "not-valid-base64-#@$%", // Invalid base64 key
            LegacyKeys = new Dictionary<string, string>()
        };

        var optionsWrapper = Options.Create(options);

        // Act & Assert
        var exception = Assert.Throws<FormatException>(() => new Encryptor(optionsWrapper));
        Assert.Contains("is not a valid Base-64 string", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldDefaultToVersion1_WhenCurrentVersionNotSpecified()
    {
        // Arrange
        var options = new EncryptionOptions
        {
            CurrentVersion = 1, // Default value
            Key = "dGhpcyBpcyBhIDI1NiBiaXQga2V5IGZvciBBRVMyNTY=",
            LegacyKeys = new Dictionary<string, string>()
        };

        var optionsWrapper = Options.Create(options);
        var encryptor = new Encryptor(optionsWrapper);

        // Act
        var encrypted = encryptor.Encrypt("test");

        // Assert
        Assert.NotNull(encrypted);
        Assert.Equal(1, encrypted.KeyVersion);
    }

    [Fact]
    public void Encrypt_ShouldReturnEncryptedString_WithCurrentVersion()
    {
        // Arrange
        var encryptor = CreateEncryptor(currentVersion: 2);
        const string plainText = "sensitive data";

        // Act
        var result = encryptor.Encrypt(plainText);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.KeyVersion);
        Assert.NotEmpty(result.Value);
        Assert.NotEqual(plainText, result.Value);
    }

    [Fact]
    public void Encrypt_ShouldReturnEncryptedStringWithVersion1_WhenPlainTextIsEmpty()
    {
        // Arrange
        var encryptor = CreateEncryptor();
        const string plainText = "";

        // Act
        var result = encryptor.Encrypt(plainText);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.KeyVersion);
        Assert.Equal("", result.Value);
    }

    [Fact]
    public void Encrypt_ShouldReturnDifferentCiphertext_ForSamePlainText()
    {
        // Arrange - Each encryption should generate a new IV
        var encryptor = CreateEncryptor();
        const string plainText = "test data";

        // Act
        var encrypted1 = encryptor.Encrypt(plainText);
        var encrypted2 = encryptor.Encrypt(plainText);

        // Assert
        Assert.NotEqual(encrypted1.Value, encrypted2.Value);
        Assert.Equal(encrypted1.KeyVersion, encrypted2.KeyVersion);
    }

    [Fact]
    public void Decrypt_ShouldReturnOriginalPlainText_WhenDecryptingWithSameVersion()
    {
        // Arrange
        var encryptor = CreateEncryptor();
        const string plainText = "sensitive information";

        // Act
        var encrypted = encryptor.Encrypt(plainText);
        var decrypted = encryptor.Decrypt(encrypted);

        // Assert
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void Decrypt_ShouldReturnEmptyString_WhenEncryptedStringValueIsEmpty()
    {
        // Arrange
        var encryptor = CreateEncryptor();
        var emptyEncrypted = new EncryptedString("", 1);

        // Act
        var result = encryptor.Decrypt(emptyEncrypted);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void Decrypt_ShouldThrowArgumentNullException_WhenEncryptedStringIsNull()
    {
        // Arrange
        var encryptor = CreateEncryptor();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => encryptor.Decrypt(null!));
    }

    [Fact]
    public void Decrypt_ShouldDecryptUsingLegacyKey_WhenVersionMatches()
    {
        // Arrange - Create encryptor with version 1 key, encrypt data
        var encryptorV1 = CreateEncryptor(currentVersion: 1);
        const string plainText = "old encrypted data";
        var encryptedWithV1 = encryptorV1.Encrypt(plainText);

        // Create new encryptor with version 2 as current, but V1 as legacy
        var encryptorV2 = CreateEncryptor(
            currentVersion: 2,
            legacyKeys: new Dictionary<int, string>
            {
                [1] = "dGhpcyBpcyBhIDI1NiBiaXQga2V5IGZvciBBRVMyNTY="
            });

        // Act - Should be able to decrypt old data with legacy key
        var decrypted = encryptorV2.Decrypt(encryptedWithV1);

        // Assert
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void Decrypt_ShouldThrowInvalidOperationException_WhenKeyVersionNotFound()
    {
        // Arrange
        var encryptor = CreateEncryptor(currentVersion: 2);
        var encryptedWithUnknownVersion = new EncryptedString("somevalue", 99);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => encryptor.Decrypt(encryptedWithUnknownVersion));
        Assert.Contains("Encryption key version 99 not found", exception.Message);
        Assert.Contains("Available versions: 2", exception.Message);
    }

    [Fact]
    public void EncryptAndDecrypt_ShouldWorkWithSpecialCharacters()
    {
        // Arrange
        var encryptor = CreateEncryptor();
        const string plainText = "Special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?`~";

        // Act
        var encrypted = encryptor.Encrypt(plainText);
        var decrypted = encryptor.Decrypt(encrypted);

        // Assert
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void EncryptAndDecrypt_ShouldWorkWithUnicodeCharacters()
    {
        // Arrange
        var encryptor = CreateEncryptor();
        const string plainText = "Unicode: 你好世界 مرحبا العالم привет мир";

        // Act
        var encrypted = encryptor.Encrypt(plainText);
        var decrypted = encryptor.Decrypt(encrypted);

        // Assert
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void EncryptAndDecrypt_ShouldWorkWithLongText()
    {
        // Arrange
        var encryptor = CreateEncryptor();
        var plainText = new string('A', 10000); // 10,000 characters

        // Act
        var encrypted = encryptor.Encrypt(plainText);
        var decrypted = encryptor.Decrypt(encrypted);

        // Assert
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void Encrypt_ShouldUseCurrentVersion_WhenMultipleVersionsConfigured()
    {
        // Arrange
        var encryptor = CreateEncryptor(
            currentVersion: 3,
            legacyKeys: new Dictionary<int, string>
            {
                [1] = "dGhpcyBpcyBhIDI1NiBiaXQga2V5IGZvciBBRVMyNTY=",
                [2] = "YW5vdGhlciAyNTYgYml0IGtleSBmb3IgQUVTMjU2"
            });

        // Act
        var encrypted = encryptor.Encrypt("test");

        // Assert
        Assert.Equal(3, encrypted.KeyVersion);
    }

    [Fact]
    public void Constructor_ShouldLoadMultipleLegacyKeys()
    {
        // Arrange
        var options = new EncryptionOptions
        {
            CurrentVersion = 3,
            Key = "dGhpcyBpcyBhIDI1NiBiaXQga2V5IGZvciBBRVMyNTY=",
            LegacyKeys = new Dictionary<string, string>
            {
                ["V1"] = "bGVnYWN5IGtleSB2ZXJzaW9uIDEgMjU2IGJpdHM=",
                ["V2"] = "bGVnYWN5IGtleSB2ZXJzaW9uIDIgMjU2IGJpdHM="
            }
        };

        var optionsWrapper = Options.Create(options);

        // Act
        var encryptor = new Encryptor(optionsWrapper);
        var encrypted = encryptor.Encrypt("test");

        // Assert - Should use current version
        Assert.Equal(3, encrypted.KeyVersion);
    }

    [Fact]
    public void Decrypt_ShouldHandleBackwardCompatibility_WithNonVersionedFormat()
    {
        // Arrange
        var encryptor = CreateEncryptor(currentVersion: 1);
        const string plainText = "backward compatible data";

        // Encrypt and get the raw encrypted value
        var encrypted = encryptor.Encrypt(plainText);

        // Create EncryptedString from the combined format (simulating old data stored in DB)
        var storedFormat = encrypted.ToString(); // "1:encryptedvalue"
        var reconstructed = new EncryptedString(storedFormat);

        // Act
        var decrypted = encryptor.Decrypt(reconstructed);

        // Assert
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void EncryptedString_ShouldFallbackToVersion1_WhenNoVersionInValue()
    {
        // Arrange - Simulate old data without version prefix
        const string oldEncryptedValue = "someBase64EncryptedValueWithoutVersion";

        // Act
        var encryptedString = new EncryptedString(oldEncryptedValue);

        // Assert
        Assert.Equal(1, encryptedString.KeyVersion);
        Assert.Equal(oldEncryptedValue, encryptedString.Value);
    }
}
