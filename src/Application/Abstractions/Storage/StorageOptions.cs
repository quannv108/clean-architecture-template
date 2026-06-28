namespace Application.Abstractions.Storage;

// Binds the "Storage" configuration section: a map of named backends, e.g. "Storage:avatars:Type".
public sealed class StorageOptions : Dictionary<string, StorageEntryOptions>;
