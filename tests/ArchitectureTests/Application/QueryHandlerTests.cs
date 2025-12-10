using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using NetArchTest.Rules;
using Shouldly;

namespace ArchitectureTests.Application;

public class QueryHandlerTests : BaseTest
{
    [Fact]
    public void QueryHandlers_Should_Implement_IQueryHandler_Interface()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("QueryHandler")
            .Should()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .GetResult();

        if (!result.IsSuccessful)
        {
            var failingHandlers = result.FailingTypeNames ?? new List<string>();
            var detailedMessage =
                $"Query handlers should implement IQueryHandler interface. Failing handlers: {string.Join(", ", failingHandlers)}";
            result.IsSuccessful.ShouldBeTrue(detailedMessage);
        }
        else
        {
            result.IsSuccessful.ShouldBeTrue("Query handlers should implement IQueryHandler interface");
        }
    }


    [Fact]
    public void QueryHandlers_Should_Be_Internal_Sealed_Classes()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("QueryHandler")
            .Should()
            .BeSealed()
            .And()
            .NotBePublic()
            .GetResult();

        if (!result.IsSuccessful)
        {
            var failingHandlers = result.FailingTypeNames ?? new List<string>();
            var detailedMessage =
                $"QueryHandlers should be internal sealed classes. Failing handlers: {string.Join(", ", failingHandlers)}";
            result.IsSuccessful.ShouldBeTrue(detailedMessage);
        }
        else
        {
            result.IsSuccessful.ShouldBeTrue("QueryHandlers should be internal sealed classes");
        }
    }
}
