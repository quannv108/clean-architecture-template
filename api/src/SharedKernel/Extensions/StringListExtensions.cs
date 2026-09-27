namespace SharedKernel.Extensions;

public static class StringListExtensions
{
    public static string StringJoin(this IEnumerable<string> strings, string separator = ", ")
    {
        return string.Join(separator, strings);
    }
}
