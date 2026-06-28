using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace Application.Abstractions.Storage;

public sealed class StorageOptionsValidator : IValidateOptions<StorageOptions>
{
    public ValidateOptionsResult Validate(string? name, StorageOptions options)
    {
        var errors = new List<string>();

        foreach (var (key, entry) in options)
        {
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(entry, new ValidationContext(entry), results, validateAllProperties: true))
            {
                errors.AddRange(results.Select(r => $"Storage:{key} - {r.ErrorMessage}"));
            }
        }

        return errors.Count > 0 ? ValidateOptionsResult.Fail(errors) : ValidateOptionsResult.Success;
    }
}
