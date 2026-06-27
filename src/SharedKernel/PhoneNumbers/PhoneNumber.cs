using System.Globalization;
using PhoneNumbers;
using LibPhoneNumber = PhoneNumbers.PhoneNumber;

namespace SharedKernel.PhoneNumbers;

public sealed class PhoneNumber : ValueObject
{
    private static readonly PhoneNumberUtil Util = PhoneNumberUtil.GetInstance();

    private PhoneNumber() { }

    /// <summary>E.164 canonical form, e.g. "+447911123456"</summary>
    public string E164 { get; private set; } = string.Empty;

    /// <summary>Calling code without leading '+', e.g. "44"</summary>
    public string CallingCode { get; private set; } = string.Empty;

    /// <summary>National significant number, e.g. "7911123456"</summary>
    public string Number { get; private set; } = string.Empty;

    /// <summary>
    /// Creates a PhoneNumber from separate calling code and local number.
    /// Accepts national-format numbers (e.g. "07911123456") and international
    /// format (e.g. "+447911123456") in the number argument.
    /// </summary>
    public static Result<PhoneNumber> Create(string callingCode, string number)
    {
        if (string.IsNullOrWhiteSpace(callingCode))
        {
            return PhoneNumberErrors.CallingCodeRequired;
        }

        if (string.IsNullOrWhiteSpace(number))
        {
            return PhoneNumberErrors.PhoneNumberRequired;
        }

        var code = callingCode.TrimStart('+');
        if (!int.TryParse(code, out var countryCode))
        {
            return PhoneNumberErrors.InvalidFormat;
        }

        var regionCode = Util.GetRegionCodeForCountryCode(countryCode);

        LibPhoneNumber parsed;
        try
        {
            parsed = Util.Parse(number, regionCode);
        }
        catch (NumberParseException)
        {
            return PhoneNumberErrors.InvalidFormat;
        }

        if (!Util.IsValidNumber(parsed))
        {
            return PhoneNumberErrors.InvalidNumber;
        }

        return FromParsed(parsed);
    }

    /// <summary>
    /// Creates a PhoneNumber from an E.164 string (must start with '+').
    /// Example: "+447911123456"
    /// </summary>
    public static Result<PhoneNumber> Create(string e164)
    {
        if (string.IsNullOrWhiteSpace(e164))
        {
            return PhoneNumberErrors.PhoneNumberRequired;
        }

        if (!e164.StartsWith('+'))
        {
            return PhoneNumberErrors.InvalidFormat;
        }

        LibPhoneNumber parsed;
        try
        {
            parsed = Util.Parse(e164, null);
        }
        catch (NumberParseException)
        {
            return PhoneNumberErrors.InvalidFormat;
        }

        if (!Util.IsValidNumber(parsed))
        {
            return PhoneNumberErrors.InvalidNumber;
        }

        return FromParsed(parsed);
    }

    private static PhoneNumber FromParsed(LibPhoneNumber parsed)
    {
        return new PhoneNumber
        {
            E164 = Util.Format(parsed, PhoneNumberFormat.E164),
            CallingCode = parsed.CountryCode.ToString(CultureInfo.InvariantCulture),
            Number = parsed.NationalNumber.ToString(CultureInfo.InvariantCulture),
        };
    }

    /// <summary>Returns calling code concatenated with number, e.g. "447911123456". No leading '+'.</summary>
    public string PhoneNumberWithCallingCode() => $"{CallingCode}{Number}";

    public override string ToString() => PhoneNumberWithCallingCode();

    protected override IEnumerable<object> GetAtomicValues() => [E164];
}
