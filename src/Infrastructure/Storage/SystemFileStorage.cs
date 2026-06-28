using Application.Abstractions.Storage;
using Microsoft.Extensions.Logging;
using SharedKernel;
using SharedKernel.Storage;

namespace Infrastructure.Storage;

internal sealed class SystemFileStorage : IStorage
{
    private const string Provider = "system";

    private readonly string _root;
    private readonly ILogger<SystemFileStorage> _logger;

    public SystemFileStorage(string rootPath, ILogger<SystemFileStorage> logger)
    {
        _root = Path.GetFullPath(rootPath);
        _logger = logger;
    }

    public async Task<Result> SaveAsync(string path, Stream content, CancellationToken cancellationToken = default)
    {
        var resolved = ResolveFullPath(path);
        if (resolved.IsFailure)
        {
            return resolved;
        }

        try
        {
            var fullPath = resolved.Value;
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var fileStream = File.Create(fullPath);
            await content.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
            return Result.Success();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            _logger.LogError(ex, "System storage failed to save path {Path}", path);
            return StorageErrors.OperationFailed(Provider, ex.Message);
        }
    }

    public async Task<Result<string>> DownloadAsync(string path,
        CancellationToken cancellationToken = default)
    {
        var resolved = ResolveFullPath(path);
        if (resolved.IsFailure)
        {
            return resolved;
        }

        try
        {
            var fullPath = resolved.Value;
            if (!File.Exists(fullPath))
            {
                return StorageErrors.NotFound(path);
            }

            return Result.Success(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            _logger.LogError(ex, "System storage failed to download path {Path}", path);
            return Result.Failure<string>(StorageErrors.OperationFailed(Provider, ex.Message));
        }
    }

    public Task<Result> DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var resolved = ResolveFullPath(path);
        if (resolved.IsFailure)
        {
            return Task.FromResult((Result)resolved);
        }

        try
        {
            var fullPath = resolved.Value;
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            // Missing path is treated as success: Delete is idempotent.
            return Task.FromResult(Result.Success());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            _logger.LogError(ex, "System storage failed to delete path {Path}", path);
            return Task.FromResult(Result.Failure(StorageErrors.OperationFailed(Provider, ex.Message)));
        }
    }

    public Task<Result<bool>> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        var resolved = ResolveFullPath(path);
        if (resolved.IsFailure)
        {
            return Task.FromResult(Result.Failure<bool>(resolved.Error));
        }

        try
        {
            return Task.FromResult(Result.Success(File.Exists(resolved.Value)));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            _logger.LogError(ex, "System storage failed to check existence of path {Path}", path);
            return Task.FromResult(Result.Failure<bool>(StorageErrors.OperationFailed(Provider, ex.Message)));
        }
    }

    private Result<string> ResolveFullPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return Result.Failure<string>(StorageErrors.InvalidPath(path));
        }

        var combined = Path.GetFullPath(Path.Combine(_root, path));

        if (combined != _root &&
            !combined.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            return Result.Failure<string>(StorageErrors.PathTraversal(path));
        }

        return combined;
    }
}
