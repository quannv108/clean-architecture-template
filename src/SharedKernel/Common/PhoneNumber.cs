namespace SharedKernel.Common;

public sealed class PhoneNumber : IEquatable<PhoneNumber>
{
    public PhoneNumber(string callingCode, string number)
    {
        if (string.IsNullOrWhiteSpace(callingCode))
        {
            throw new ArgumentException("Calling code cannot be null or empty.", nameof(callingCode));
        }

        if (string.IsNullOrWhiteSpace(number))
        {
            throw new ArgumentException("Phone number cannot be null or empty.", nameof(number));
        }

        CallingCode = callingCode.TrimStart('+');
        Number = number.StartsWith('+')
            ? number.TrimStart('+').TrimStart(CallingCode.ToCharArray())
            : number.TrimStart('0');
    }

    /// <summary>
    /// Calling code without leading '+' sign, e.g. "1" for +1
    /// </summary>
    public string CallingCode { get; }

    /// <summary>
    /// Number without leading zeros or calling code, e.g. "1234567890"
    /// </summary>
    public string Number { get; }

    public string PhoneNumberWithCallingCode()
    {
        return $"{CallingCode}{Number}";
    }

    public override string ToString()
    {
        return PhoneNumberWithCallingCode();
    }

    public override bool Equals(object? obj)
    {
        return ToString().Equals(obj?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public bool Equals(PhoneNumber? other)
    {
        return ToString().Equals(other?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CallingCode, Number);
    }
}
