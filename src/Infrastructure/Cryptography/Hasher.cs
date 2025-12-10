using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Cryptography;

namespace Infrastructure.Cryptography;

internal sealed class Hasher : IHasher
{
    public string Hash(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = SHA512.HashData(inputBytes);

        return Convert.ToHexString(hashBytes);
    }
}
