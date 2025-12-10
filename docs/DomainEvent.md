# Domain Events

Domain events are a critical part of the Clean Architecture template, enabling decoupled communication between different parts of the system. They represent significant occurrences within the domain that other parts of the system may need to react to.

## Key Concepts

### What is a Domain Event?
A domain event is an event that signifies something important has happened within the domain. For example, a `UserRegistered` event might be raised when a new user is successfully registered.

### Purpose
- **Decoupling**: Allows different parts of the system to react to events without tight coupling.
- **Cross-cutting concerns**: Enables handling of concerns like logging, notifications, or analytics in a centralized manner.
- **Business logic orchestration**: Facilitates complex workflows by chaining events.

## Implementation

### Domain Event Interface
All domain events must implement the `IDomainEvent` interface, which is defined in the `SharedKernel` layer.

```csharp
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
```

### Raising Domain Events
Domain events are typically raised by aggregate roots. For example:

```csharp
public class User : Entity
{
    public void Register()
    {
        // Business logic for registration
        RaiseDomainEvent(new UserRegistered(this));
    }
}
```

### Handling Domain Events
Domain events are handled by domain event handlers, which implement the `IDomainEventHandler<T>` interface.

```csharp
public class UserRegisteredHandler : IDomainEventHandler<UserRegistered>
{
    public Task Handle(UserRegistered domainEvent, CancellationToken cancellationToken)
    {
        // Handle the event (e.g., send a welcome email)
        return Task.CompletedTask;
    }
}
```

## Best Practices
- **Keep domain events lightweight**: Only include necessary data.
- **Avoid side effects**: Domain events should not modify state.
- **Test thoroughly**: Ensure domain events and their handlers work as expected.

## Examples

### Example Domain Event
```csharp
public class UserRegistered : IDomainEvent
{
    public UserRegistered(User user)
    {
        User = user;
        OccurredOn = DateTime.UtcNow;
    }

    public User User { get; }
    public DateTime OccurredOn { get; }
}
```

### Example Handler
```csharp
public class UserRegisteredHandler : IDomainEventHandler<UserRegistered>
{
    public Task Handle(UserRegistered domainEvent, CancellationToken cancellationToken)
    {
        Console.WriteLine($"User {domainEvent.User.Id} registered at {domainEvent.OccurredOn}");
        return Task.CompletedTask;
    }
}
```

## Conclusion
Domain events are a powerful tool for building scalable and maintainable systems. By following the principles outlined here, you can ensure your domain events are effective and align with Clean Architecture best practices.
