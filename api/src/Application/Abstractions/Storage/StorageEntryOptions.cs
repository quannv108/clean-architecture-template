using System.ComponentModel.DataAnnotations;

namespace Application.Abstractions.Storage;

public sealed class StorageEntryOptions : IValidatableObject
{
    private static readonly string[] KnownTypes = ["system", "s3", "sftp"];

    // "system" | "s3" | "sftp" (case-insensitive). s3/sftp are reserved extension points (no SDK yet).
    public string Type { get; set; } = "";

    // Required when Type == "system".
    public string? RootPath { get; set; }

    // S3/SFTP-specific fields (Bucket, ServiceUrl, AccessKey?, SecretKey?, Host, Username, Password)
    // will be added when those backends are implemented.

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!KnownTypes.Contains(Type, StringComparer.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                $"Storage Type '{Type}' is not recognized. Expected one of: {string.Join(", ", KnownTypes)}.",
                [nameof(Type)]);
        }

        if (Type.Equals("system", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(RootPath))
        {
            yield return new ValidationResult(
                "RootPath is required when Type is 'system'.",
                [nameof(RootPath)]);
        }
    }
}
