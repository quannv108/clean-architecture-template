using SharedKernel;

namespace Application.Abstractions.Cryptography;

public interface IEncryptor
{
    EncryptedString Encrypt(string plainText);
    string Decrypt(EncryptedString encryptedString);
}
