using System.Text;
using System.Text.RegularExpressions;

namespace SharedKernel.Extensions;

public static class StringExtensions
{
    public static string MaskEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return email;
        }

        var atIndex = email.IndexOf('@', StringComparison.Ordinal);
        if (atIndex == -1)
        {
            return email; // Not a valid email format
        }

        var localPart = email.Substring(0, atIndex);
        var domainPart = email.Substring(atIndex);

        // If local part is too short, just mask with asterisks
        if (localPart.Length <= 4)
        {
            return new string('*', localPart.Length) + domainPart;
        }

        // Show first 4 characters + asterisks + domain
        var maskedLocalPart = string.Concat(localPart.AsSpan(0, 4), new string('*', localPart.Length - 4));

        // For domain, show last 6 characters if possible
        if (domainPart.Length > 6)
        {
            var domainToMask = domainPart.Substring(1, domainPart.Length - 7); // Skip @ and last 6 chars
            var lastSixChars = domainPart.Substring(domainPart.Length - 6);
            var maskedDomain = "@" + new string('*', domainToMask.Length) + lastSixChars;
            return maskedLocalPart + maskedDomain;
        }

        return maskedLocalPart + domainPart;
    }

    public static string MaskPhoneNumber(this string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return phoneNumber;
        }

        // Assuming phone number is in the format +1234567890 or 1234567890
        var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());

        if (digitsOnly.Length < 4)
        {
            return new string('*', digitsOnly.Length);
        }

        // Show last 4 digits and mask the rest
        var maskedPart = new string('*', digitsOnly.Length - 4);
        var visiblePart = digitsOnly.Substring(digitsOnly.Length - 4);

        return maskedPart + visiblePart;
    }

    public static Guid ToGuid(this string str)
    {
        if (Guid.TryParse(str, out var guid))
        {
            return guid;
        }

        throw new FormatException("Invalid Guid format");
    }

    public static string ToKebabCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        // Insert hyphens before uppercase letters or digits following lowercase letters
        string kebab = Regex.Replace(
            value,
            @"(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])",
            "-$1",
            RegexOptions.Compiled
        );

        return kebab.ToLowerInvariant();
    }

    public static string ToSnakeCase(this string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append(char.ToLowerInvariant(text[0]));

        for (int i = 1; i < text.Length; i++)
        {
            char c = text[i];
            if (char.IsUpper(c))
            {
                sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    public static string ToPlural(this string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return word;
        }

        string lower = word.ToLowerInvariant();

        // Handle some irregular plurals
        var irregulars = new Dictionary<string, string>
        {
            { "child", "children" },
            { "person", "people" },
            { "man", "men" },
            { "woman", "women" },
            { "mouse", "mice" },
            { "goose", "geese" },
            { "tooth", "teeth" },
            { "foot", "feet" },
            { "ox", "oxen" }
        };

        if (irregulars.TryGetValue(lower, out string? value))
        {
            return value;
        }

        // Handle common endings
        if (lower.EndsWith('y') && word.Length > 1 && !"aeiou".Contains(lower[^2]))
        {
            return string.Concat(word.AsSpan(0, word.Length - 1), "ies");
        }

        if (lower.EndsWith('s') || lower.EndsWith('x') || lower.EndsWith('z') ||
            lower.EndsWith("ch", StringComparison.Ordinal) || lower.EndsWith("sh", StringComparison.Ordinal))
        {
            return word + "es";
        }

        return word + "s";
    }
}
