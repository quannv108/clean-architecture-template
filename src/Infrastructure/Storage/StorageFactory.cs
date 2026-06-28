using System.Collections.Concurrent;
using Application.Abstractions.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;
using SharedKernel.Storage;

namespace Infrastructure.Storage;

internal sealed class StorageFactory : IStorageFactory
{
    private readonly StorageOptions _options;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ConcurrentDictionary<string, Lazy<IStorage>> _cache = new(StringComparer.Ordinal);

    public StorageFactory(IOptions<StorageOptions> options, ILoggerFactory loggerFactory)
    {
        _options = options.Value;
        _loggerFactory = loggerFactory;
    }

    public IStorage Create(string key) =>
        _cache.GetOrAdd(key, k =>
            new Lazy<IStorage>(() => Build(k), LazyThreadSafetyMode.ExecutionAndPublication)).Value;

#pragma warning disable CA1859 // Use concrete types when possible for improved performance

    private IStorage Build(string key)
#pragma warning restore CA1859 // remove when have another implementation of IStorage

    {
        if (!_options.TryGetValue(key, out var entry))
        {
            throw new InvalidOperationException(StorageErrors.KeyNotConfigured(key).Description);
        }

        return entry.Type.ToLowerInvariant() switch
        {
            "system" => new SystemFileStorage(entry.RootPath!, _loggerFactory.CreateLogger<SystemFileStorage>()),
            // s3/sftp are recognized config types reserved for later: no SDK dependencies are added yet,
            // so they resolve to a stub that reports "not implemented" per operation instead of a backend.
            "s3" or "sftp" => throw new NotImplementedException("Not Implemented"),
            _ => throw new InvalidOperationException(StorageErrors.KeyNotConfigured(key).Description)
        };
    }
}
