namespace Application.Abstractions.Cryptography;

public interface IHasher
{
    string Hash(string input);
}
