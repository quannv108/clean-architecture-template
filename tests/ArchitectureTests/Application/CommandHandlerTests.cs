using Application.Abstractions.Messaging;
using NetArchTest.Rules;
using Shouldly;

namespace ArchitectureTests.Application;

public class CommandHandlerTests : BaseTest
{
    [Fact]
    public void CommandHandlers_Should_Implement_ICommandHandler_Interface()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("CommandHandler")
            .Should()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .GetResult();

        if (!result.IsSuccessful)
        {
            var failingHandlers = result.FailingTypeNames ?? new List<string>();
            var detailedMessage =
                $"Command handlers should implement ICommandHandler interface. Failing handlers: {string.Join(", ", failingHandlers)}";
            result.IsSuccessful.ShouldBeTrue(detailedMessage);
        }
        else
        {
            result.IsSuccessful.ShouldBeTrue("Command handlers should implement ICommandHandler interface");
        }
    }

    [Fact]
    public void CommandHandlers_Should_Be_Internal_Sealed_Classes()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("CommandHandler")
            .Should()
            .BeSealed()
            .And()
            .NotBePublic()
            .GetResult();

        if (!result.IsSuccessful)
        {
            var failingHandlers = result.FailingTypeNames ?? new List<string>();
            var detailedMessage =
                $"CommandHandlers should be internal sealed classes. Failing handlers: {string.Join(", ", failingHandlers)}";
            result.IsSuccessful.ShouldBeTrue(detailedMessage);
        }
        else
        {
            result.IsSuccessful.ShouldBeTrue("CommandHandlers should be internal sealed classes");
        }
    }
}
