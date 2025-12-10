namespace SharedKernel;

public static class EntityIdGenerator
{
    public static Guid NewId()
    {
        return Guid.CreateVersion7();
    }
}
