using System.ComponentModel.DataAnnotations;

namespace Application.Abstractions.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class RegularIdAttribute : ValidationAttribute
{
    private static readonly Guid MinValue = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid MaxValue = new("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");

    public RegularIdAttribute()
    {
        ErrorMessage = "Invalid ID format";
    }

    public override bool IsValid(object? value)
    {
        if (value is Guid guid)
        {
            return guid.CompareTo(MinValue) >= 0 && guid.CompareTo(MaxValue) <= 0;
        }

        return false;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"The {name} field must be a valid Id";
    }
}
