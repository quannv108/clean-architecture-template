using Application.Abstractions.Storage;
using Infrastructure.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Storage;

namespace Application.UnitTests.Storage;

public sealed class SystemFileStorageTests : IDisposable
{
    private readonly string _root;
    private readonly SystemFileStorage _storage;

    public SystemFileStorageTests()
    {
        _root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_root);
        _storage = new SystemFileStorage(_root, NullLogger<SystemFileStorage>.Instance);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public async Task SaveExistsDownload_ShouldRoundTrip_ByteIdenticalContent()
    {
        // Arrange
        var bytes = "hello world"u8.ToArray();
        using var source = new MemoryStream(bytes);

        // Act
        var saveResult = await _storage.SaveAsync("a/b/file.txt", source);
        var existsResult = await _storage.ExistsAsync("a/b/file.txt");
        var downloadResult = await _storage.DownloadAsync("a/b/file.txt");

        // Assert
        saveResult.IsSuccess.ShouldBeTrue();
        existsResult.IsSuccess.ShouldBeTrue();
        existsResult.Value.ShouldBeTrue();
        downloadResult.IsSuccess.ShouldBeTrue();
        var downloadContent = await File.ReadAllBytesAsync(downloadResult.Value);
        downloadContent.ToArray().ShouldBe(bytes);
    }

    [Fact]
    public async Task SaveAsync_ShouldOverwriteExistingFile()
    {
        // Arrange
        using var first = new MemoryStream("first"u8.ToArray());
        using var second = new MemoryStream("second-content"u8.ToArray());

        // Act
        await _storage.SaveAsync("file.txt", first);
        await _storage.SaveAsync("file.txt", second);

        var downloadPath = await _storage.DownloadAsync("file.txt");
        var downloadContent = await File.ReadAllBytesAsync(downloadPath.Value);

        // Assert
        downloadContent.ToArray().ShouldBe("second-content"u8.ToArray());
    }

    [Fact]
    public async Task SaveAsync_ShouldCreateMissingParentDirectories()
    {
        // Arrange
        using var content = new MemoryStream("nested"u8.ToArray());

        // Act
        var result = await _storage.SaveAsync("deeply/nested/path/file.txt", content);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        File.Exists(Path.Combine(_root, "deeply", "nested", "path", "file.txt")).ShouldBeTrue();
    }

    [Fact]
    public async Task DownloadAsync_ShouldReturnNotFound_WhenPathMissing()
    {
        // Act
        using var destination = new MemoryStream();
        var downloadPath = await _storage.DownloadAsync("missing.txt");

        // Assert
        downloadPath.IsFailure.ShouldBeTrue();
        downloadPath.Error.ShouldBe(StorageErrors.NotFound("missing.txt"));
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveFile()
    {
        // Arrange
        using var content = new MemoryStream("to-delete"u8.ToArray());
        await _storage.SaveAsync("file.txt", content);

        // Act
        var deleteResult = await _storage.DeleteAsync("file.txt");
        var existsResult = await _storage.ExistsAsync("file.txt");

        // Assert
        deleteResult.IsSuccess.ShouldBeTrue();
        existsResult.Value.ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldSucceed_WhenPathMissing()
    {
        // Act
        var result = await _storage.DeleteAsync("never-existed.txt");

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenAbsent()
    {
        // Act
        var result = await _storage.ExistsAsync("absent.txt");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenPresent()
    {
        // Arrange
        using var content = new MemoryStream("present"u8.ToArray());
        await _storage.SaveAsync("present.txt", content);

        // Act
        var result = await _storage.ExistsAsync("present.txt");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
    }

    [Theory]
    [InlineData("../outside.txt")]
    [InlineData("a/../../b")]
    public async Task ResolveFullPath_ShouldReturnPathTraversal_ForRelativeEscapes(string path)
    {
        // Act
        using var content = new MemoryStream("malicious"u8.ToArray());
        var saveResult = await _storage.SaveAsync(path, content);
        using var destination = new MemoryStream();
        var downloadResult = await _storage.DownloadAsync(path);

        // Assert
        saveResult.IsFailure.ShouldBeTrue();
        saveResult.Error.ShouldBe(StorageErrors.PathTraversal(path));
        downloadResult.IsFailure.ShouldBeTrue();
        downloadResult.Error.ShouldBe(StorageErrors.PathTraversal(path));

        var parentDir = Path.GetDirectoryName(_root);
        if (!string.IsNullOrEmpty(parentDir))
        {
            File.Exists(Path.Combine(parentDir, "outside.txt")).ShouldBeFalse();
            File.Exists(Path.Combine(parentDir, "b")).ShouldBeFalse();
        }
    }

    [Fact]
    public async Task SaveAsync_ShouldReturnPathTraversal_ForAbsolutePathOutsideRoot()
    {
        // Arrange: an absolute path elsewhere on disk, outside the storage root.
        var outsidePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "-outside.txt");
        using var content = new MemoryStream("malicious"u8.ToArray());

        // Act
        var result = await _storage.SaveAsync(outsidePath, content);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(StorageErrors.PathTraversal(outsidePath));
        File.Exists(outsidePath).ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SaveAsync_ShouldReturnInvalidPath_ForBlankPath(string path)
    {
        // Act
        using var content = new MemoryStream("x"u8.ToArray());
        var result = await _storage.SaveAsync(path, content);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(StorageErrors.InvalidPath(path));
    }
}
