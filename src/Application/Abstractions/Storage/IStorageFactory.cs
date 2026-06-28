namespace Application.Abstractions.Storage;

public interface IStorageFactory
{
    /// <summary>
    /// Resolves the named storage backend configured under "Storage:{key}".
    /// </summary>
    /// <exception cref="InvalidOperationException">The key is not configured or misconfigured.</exception>
    IStorage Create(string key);
}
