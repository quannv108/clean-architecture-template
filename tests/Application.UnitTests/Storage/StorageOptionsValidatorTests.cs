using Application.Abstractions.Storage;

namespace Application.UnitTests.Storage;

public class StorageOptionsValidatorTests
{
    private readonly StorageOptionsValidator _validator = new();

    [Fact]
    public void Validate_ShouldFail_WhenSystemEntryMissingRootPath()
    {
        // Arrange
        var options = new StorageOptions
        {
            ["avatars"] = new StorageEntryOptions { Type = "system", RootPath = null }
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldNotBeNull();
        result.FailureMessage.ShouldContain("RootPath is required");
    }

    [Fact]
    public void Validate_ShouldFail_WhenTypeIsUnknown()
    {
        // Arrange
        var options = new StorageOptions
        {
            ["weird"] = new StorageEntryOptions { Type = "ftp", RootPath = "/tmp" }
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldNotBeNull();
        result.FailureMessage.ShouldContain("is not recognized");
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenSystemEntryHasRootPath()
    {
        // Arrange
        var options = new StorageOptions
        {
            ["avatars"] = new StorageEntryOptions { Type = "system", RootPath = "/data/avatars" }
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Theory]
    [InlineData("s3")]
    [InlineData("sftp")]
    [InlineData("S3")]
    [InlineData("SFTP")]
    public void Validate_ShouldSucceed_ForKnownNonSystemTypes(string type)
    {
        // Arrange
        var options = new StorageOptions
        {
            ["backups"] = new StorageEntryOptions { Type = type }
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }
}
