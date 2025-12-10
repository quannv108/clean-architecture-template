namespace SharedKernel.Extensions;

public static class EnumExtensions
{
    public static bool IsValidEnumValue<TEnum>(this string value) where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(value, true, out _);
    }

    public static bool IsValidEnumValue<TEnum>(this int value) where TEnum : struct, Enum
    {
        return Enum.IsDefined(typeof(TEnum), value);
    }
}
