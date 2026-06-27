using Application.Abstractions.Authentication;
using NetArchTest.Rules;
using SharedKernel;
using Shouldly;

namespace ArchitectureTests.Application;

public class DomainEventHandlerTests : BaseTest
{
    [Fact]
    public void DomainEventHandlers_ShouldNot_Depend_On_IUserContext()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IDomainEventHandler<>))
            .And()
            .DoNotResideInNamespace("Application.Abstractions.Behaviors")
            .ShouldNot()
            .HaveDependencyOn(typeof(IUserContext).FullName!)
            .GetResult();

        if (!result.IsSuccessful)
        {
            var failing = result.FailingTypeNames ?? new List<string>();
            result.IsSuccessful.ShouldBeTrue(
                $"DomainEventHandlers must not inject IUserContext. DomainEventsDispatcher sets SystemUserId automatically. Failing: {string.Join(", ", failing)}");
        }
    }
}
