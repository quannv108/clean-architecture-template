using SharedKernel.Extensions;

namespace Application.UnitTests.Extensions;

public sealed class StringExtensionsTests
{
    // -------------------------------------------------------------------------
    // ToKebabCase — behavior must be unchanged after adding the regex timeout
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("PascalCase", "pascal-case")]
    [InlineData("camelCase", "camel-case")]
    [InlineData("AuditLogs", "audit-logs")]
    [InlineData("HTTPServer", "http-server")]
    [InlineData("Version2Update", "version-2-update")]
    [InlineData("already-kebab", "already-kebab")]
    [InlineData("single", "single")]
    [InlineData("A", "a")]
    public void ToKebabCase_ShouldConvertToKebabCase(string input, string expected)
    {
        input.ToKebabCase().ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ToKebabCase_WhenNullOrWhitespace_ShouldReturnEmpty(string input)
    {
        input.ToKebabCase().ShouldBe(string.Empty);
    }

    [Fact]
    public void ToKebabCase_WhenInputIsLong_ShouldCompleteWithinRegexTimeout()
    {
        var longInput = string.Concat(Enumerable.Repeat("SomePascalSegment1", 500));

        var result = longInput.ToKebabCase();

        result.ShouldStartWith("some-pascal-segment-1");
    }
}
