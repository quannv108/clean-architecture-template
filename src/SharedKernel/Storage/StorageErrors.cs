namespace SharedKernel.Storage;

public static class StorageErrors
{
    public static Error InvalidPath(string path) => Error.Validation(
        "Storage.InvalidPath",
        $"The path '{path}' is invalid.");

    public static Error PathTraversal(string path) => Error.Validation(
        "Storage.PathTraversal",
        $"The path '{path}' resolves outside the storage root.");

    public static Error NotFound(string path) => Error.NotFound(
        "Storage.NotFound",
        $"No file was found at path '{path}'.");

    public static Error OperationFailed(string provider, string message) => Error.Failure(
        "Storage.OperationFailed",
        $"{provider} storage operation failed: {message}");

    public static Error KeyNotConfigured(string key) => Error.Failure(
        "Storage.KeyNotConfigured",
        $"No storage backend is configured for key '{key}'.");
}
