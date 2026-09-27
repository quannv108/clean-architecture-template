using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SharedKernel;

namespace Infrastructure.Database.Converters;

/// <summary>
/// EF Core value converter for EncryptedString.
/// Converts EncryptedString to string (format: "version:encryptedValue") for database storage.
/// </summary>
internal sealed class EncryptedStringConverter : ValueConverter<EncryptedString, string>
{
    public EncryptedStringConverter()
        : base(
            encryptedString => encryptedString.ToString(),
            str => new EncryptedString(str))
    {
    }
}
