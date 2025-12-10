namespace SharedKernel;

public static class ConcurrencyErrors
{
    public static Error UpdateConflict(string entityType, Guid id) => Error.Conflict(
        "Concurrency.UpdateConflict",
        $"The {entityType} with ID '{id}' was modified by another user. Please refresh and try again.");

    public static Error DeleteConflict(string entityType, Guid id) => Error.Conflict(
        "Concurrency.DeleteConflict",
        $"The {entityType} with ID '{id}' was deleted or modified by another user.");

    public static Error UpdateConflict(string entityType) => Error.Conflict(
        "Concurrency.UpdateConflict",
        $"One or more {entityType} entities were modified by another user. Please refresh and try again.");
}
