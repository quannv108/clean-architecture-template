using NetArchTest.Rules;
using SharedKernel;
using Shouldly;

namespace ArchitectureTests.Domain;

public class DomainEventTests : BaseTest
{
    [Fact]
    public void Domain_Events_Should_Implement_IDomainEvent()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .That()
            .HaveNameEndingWith("DomainEvent")
            .Should()
            .ImplementInterface(typeof(IDomainEvent))
            .GetResult();

        result.IsSuccessful.ShouldBeTrue("Domain events should implement IDomainEvent interface");
    }

    [Fact]
    public void Every_DomainEvent_Should_Have_At_Least_One_Handler()
    {
        var domainEventType = typeof(IDomainEvent);
        var handlerGenericType = typeof(IDomainEventHandler<>);

        // Find all domain event types (non-abstract, non-interface, implements IDomainEvent)
        var domainEventTypes = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(domainEventType)
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes();

        // Find all handler types (non-abstract, non-interface, implements IDomainEventHandler<X>)
        var handlerTypes = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IDomainEventHandler<>))
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes();

        var missingHandlers = new List<string>();

        foreach (var eventType in domainEventTypes)
        {
            IEnumerable<Type> enumerable = handlerTypes.ToList();
            bool hasHandler = enumerable.Any(handlerType =>
                handlerType.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == handlerGenericType &&
                    i.GetGenericArguments()[0] == eventType));
            if (!hasHandler)
            {
                missingHandlers.Add(eventType.Name);
            }
        }

        if (missingHandlers.Any())
        {
            var message = $"Missing handler(s) for domain event(s): {string.Join(", ", missingHandlers)}";
            missingHandlers.ShouldBeEmpty(message);
        }
    }

    [Fact]
    public void DomainEvent_Should_Not_Have_Entity_Properties()
    {
        var domainEventType = typeof(IDomainEvent);
        var entityType = typeof(Entity);
        var domainEventTypes = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(domainEventType)
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes();

        var violations = new List<string>();
        foreach (var eventType in domainEventTypes)
        {
            var entityProperties = eventType.GetProperties()
                .Where(p => entityType.IsAssignableFrom(p.PropertyType));
            foreach (var prop in entityProperties)
            {
                violations.Add($"{eventType.Name}.{prop.Name}");
            }
        }

        violations.Count.ShouldBe(0,
            $"Domain events must not have properties of type Entity. Violations: {string.Join(", ", violations)}");
    }
}
