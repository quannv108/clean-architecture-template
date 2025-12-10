namespace Web.Api.Infrastructure;

[AttributeUsage(AttributeTargets.Method)]
public sealed class AuditAttribute(string actionName) : Attribute
{
    public string ActionName { get; } = actionName;
}
