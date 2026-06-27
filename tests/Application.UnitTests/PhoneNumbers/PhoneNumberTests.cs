using SharedKernel.PhoneNumbers;

namespace Application.UnitTests.PhoneNumbers;

public sealed class PhoneNumberTests
{
    // -------------------------------------------------------------------------
    // Create — validation
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_WhenCallingCodeIsEmpty_ShouldReturnFailure()
    {
        var result = PhoneNumber.Create("", "1234567890");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("PhoneNumber.CallingCodeRequired");
    }

    [Fact]
    public void Create_WhenCallingCodeIsWhitespace_ShouldReturnFailure()
    {
        var result = PhoneNumber.Create("   ", "1234567890");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("PhoneNumber.CallingCodeRequired");
    }

    [Fact]
    public void Create_WhenNumberIsEmpty_ShouldReturnFailure()
    {
        var result = PhoneNumber.Create("1", "");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("PhoneNumber.PhoneNumberRequired");
    }

    [Fact]
    public void Create_WhenNumberIsWhitespace_ShouldReturnFailure()
    {
        var result = PhoneNumber.Create("1", "   ");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("PhoneNumber.PhoneNumberRequired");
    }

    [Fact]
    public void Create_WhenInputsAreValid_ShouldReturnSuccess()
    {
        var result = PhoneNumber.Create("44", "7911123456");

        result.IsSuccess.ShouldBeTrue();
    }

    // -------------------------------------------------------------------------
    // Create — normalization
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_WhenCallingCodeHasLeadingPlus_ShouldStripIt()
    {
        var phone = PhoneNumber.Create("+44", "7911123456").Value;

        phone.CallingCode.ShouldBe("44");
    }

    [Fact]
    public void Create_WhenNumberHasLeadingZero_NationalFormat_ShouldNormalize()
    {
        // UK national format "07911123456" normalises to "7911123456"
        var phone = PhoneNumber.Create("44", "07911123456").Value;

        phone.Number.ShouldBe("7911123456");
    }

    [Fact]
    public void Create_WhenNumberStartsWithPlusAndCallingCode_ShouldStripBoth()
    {
        // +44 7911123456 in international format: +447911123456
        // TrimStart strips any leading chars that appear in the calling code char set
        var phone = PhoneNumber.Create("44", "+447911123456").Value;

        phone.Number.ShouldBe("7911123456");
    }

    // -------------------------------------------------------------------------
    // Equality — same value
    // -------------------------------------------------------------------------

    [Fact]
    public void Equals_WhenBothHaveSameCallingCodeAndNumber_ShouldBeTrue()
    {
        var a = PhoneNumber.Create("44", "7911123456").Value;
        var b = PhoneNumber.Create("44", "7911123456").Value;

        a.Equals(b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WhenCallingCodesDifferByLeadingPlus_ShouldBeTrue()
    {
        var a = PhoneNumber.Create("+44", "7911123456").Value;
        var b = PhoneNumber.Create("44", "7911123456").Value;

        a.Equals(b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WhenNumbersDifferByLeadingZeros_ShouldBeTrue()
    {
        // UK national "07911123456" and international "7911123456" normalise to same E.164
        var a = PhoneNumber.Create("44", "07911123456").Value;
        var b = PhoneNumber.Create("44", "7911123456").Value;

        a.Equals(b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WhenOneNumberInInternationalFormat_ShouldBeTrue()
    {
        // "+447911123456" and "7911123456" normalize to the same Number under calling code "44"
        var international = PhoneNumber.Create("44", "+447911123456").Value;
        var local = PhoneNumber.Create("44", "7911123456").Value;

        international.Equals(local).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WhenCreatedFromDifferentOverloads_WithSameNumber_ShouldBeTrue()
    {
        var fromTwoArgs = PhoneNumber.Create("44", "7911123456").Value;
        var fromE164    = PhoneNumber.Create("+447911123456").Value;

        fromTwoArgs.Equals(fromE164).ShouldBeTrue();
    }

    [Fact]
    public void Equals_SameInstance_ShouldBeTrue()
    {
        var phone = PhoneNumber.Create("44", "7911123456").Value;

        phone.Equals(phone).ShouldBeTrue();
    }

    [Fact]
    public void EqualsOperator_ViaObjectEquals_WhenSameValues_ShouldBeTrue()
    {
        var a = PhoneNumber.Create("44", "7911123456").Value;
        var b = PhoneNumber.Create("44", "7911123456").Value;

        a.Equals((object)b).ShouldBeTrue();
    }

    // -------------------------------------------------------------------------
    // Equality — different values
    // -------------------------------------------------------------------------

    [Fact]
    public void Equals_WhenCallingCodesDiffer_ShouldBeFalse()
    {
        var a = PhoneNumber.Create("44", "7911123456").Value;
        var b = PhoneNumber.Create("1", "2015550123").Value;

        a.Equals(b).ShouldBeFalse();
    }

    [Fact]
    public void Equals_WhenNumbersDiffer_ShouldBeFalse()
    {
        var a = PhoneNumber.Create("44", "7911123456").Value;
        var b = PhoneNumber.Create("44", "7400123456").Value;

        a.Equals(b).ShouldBeFalse();
    }

    [Fact]
    public void Equals_WhenComparedToNull_ShouldBeFalse()
    {
        var phone = PhoneNumber.Create("44", "7911123456").Value;

        phone.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void Equals_WhenComparedToDifferentType_ShouldBeFalse()
    {
        var phone = PhoneNumber.Create("44", "7911123456").Value;

        phone.Equals("+447911123456").ShouldBeFalse();
    }

    // -------------------------------------------------------------------------
    // GetHashCode
    // -------------------------------------------------------------------------

    [Fact]
    public void GetHashCode_WhenPhoneNumbersAreEqual_ShouldReturnSameHashCode()
    {
        var a = PhoneNumber.Create("44", "7911123456").Value;
        var b = PhoneNumber.Create("44", "7911123456").Value;

        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WhenNormalizedToSameValue_ShouldReturnSameHashCode()
    {
        var a = PhoneNumber.Create("+44", "07911123456").Value;
        var b = PhoneNumber.Create("44", "7911123456").Value;

        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WhenPhoneNumbersDiffer_ShouldReturnDifferentHashCodes()
    {
        var a = PhoneNumber.Create("44", "7911123456").Value;
        var b = PhoneNumber.Create("1", "2015550123").Value;

        a.GetHashCode().ShouldNotBe(b.GetHashCode());
    }

    // -------------------------------------------------------------------------
    // ToString / PhoneNumberWithCallingCode
    // -------------------------------------------------------------------------

    [Fact]
    public void ToString_ShouldReturnCallingCodeConcatenatedWithNumber()
    {
        var phone = PhoneNumber.Create("44", "7911123456").Value;

        phone.ToString().ShouldBe("447911123456");
    }

    [Fact]
    public void PhoneNumberWithCallingCode_ShouldMatchToString()
    {
        var phone = PhoneNumber.Create("44", "7911123456").Value;

        phone.PhoneNumberWithCallingCode().ShouldBe(phone.ToString());
    }

    // -------------------------------------------------------------------------
    // Create — format validation
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_FromE164_WhenMissingPlus_ShouldReturnInvalidFormat()
    {
        var result = PhoneNumber.Create("447911123456");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("PhoneNumber.InvalidFormat");
    }

    [Fact]
    public void Create_WhenCallingCodeIsNotNumeric_ShouldReturnInvalidFormat()
    {
        var result = PhoneNumber.Create("abc", "7911123456");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("PhoneNumber.InvalidFormat");
    }

    // -------------------------------------------------------------------------
    // Create (E.164 overload)
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_FromE164_WhenEmpty_ShouldReturnPhoneNumberRequired()
    {
        var result = PhoneNumber.Create("");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("PhoneNumber.PhoneNumberRequired");
    }

    [Fact]
    public void Create_FromE164_WhenWhitespace_ShouldReturnPhoneNumberRequired()
    {
        var result = PhoneNumber.Create("   ");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("PhoneNumber.PhoneNumberRequired");
    }

    [Fact]
    public void Create_FromE164_WhenInvalidString_ShouldReturnInvalidFormat()
    {
        var result = PhoneNumber.Create("+notaphonenumber");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("PhoneNumber.InvalidFormat");
    }

    [Fact]
    public void Create_FromE164_WhenValidUkMobile_ShouldSucceed()
    {
        var result = PhoneNumber.Create("+447911123456");

        result.IsSuccess.ShouldBeTrue();
        result.Value.E164.ShouldBe("+447911123456");
        result.Value.CallingCode.ShouldBe("44");
        result.Value.Number.ShouldBe("7911123456");
    }

    [Fact]
    public void Create_FromE164_WhenValidUsNumber_ShouldSucceed()
    {
        var result = PhoneNumber.Create("+12015550123");

        result.IsSuccess.ShouldBeTrue();
        result.Value.E164.ShouldBe("+12015550123");
        result.Value.CallingCode.ShouldBe("1");
    }

    // -------------------------------------------------------------------------
    // Create — InvalidNumber (parseable but not a valid number)
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_TwoArg_WhenNumberIsParsableButInvalid_ShouldReturnInvalidNumber()
    {
        // UK calling code "44", number "1234567" — too short to be a valid UK number
        var result = PhoneNumber.Create("44", "1234567");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("PhoneNumber.InvalidNumber");
    }

    [Fact]
    public void Create_FromE164_WhenNumberIsParsableButInvalid_ShouldReturnInvalidNumber()
    {
        // "+441234567" — UK prefix but too short to be valid
        var result = PhoneNumber.Create("+441234567");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("PhoneNumber.InvalidNumber");
    }

    // -------------------------------------------------------------------------
    // Create (two-arg overload) — E164 property
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_TwoArg_WhenInputsAreValid_ShouldSetE164Property()
    {
        var phone = PhoneNumber.Create("44", "7911123456").Value;

        phone.E164.ShouldBe("+447911123456");
    }
}
