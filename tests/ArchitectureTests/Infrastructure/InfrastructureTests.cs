using NetArchTest.Rules;
using Shouldly;

namespace ArchitectureTests.Infrastructure;

public class InfrastructureTests : BaseTest
{
    [Fact]
    public void Infrastructure_Should_Not_Reference_Web_Layer()
    {
        TestResult result = Types.InAssembly(InfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn(PresentationAssembly.GetName().Name)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue("Infrastructure should not reference Web layer");
    }

    [Fact]
    public void Repository_Implementations_Should_Be_Internal()
    {
        TestResult result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .NotBePublic()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue("Repository implementations should be internal");
    }

    [Fact]
    public void Infrastructure_Services_Should_Be_Internal()
    {
        TestResult result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .ResideInNamespaceMatching(@"Infrastructure\..*")
            .And()
            .AreClasses()
            .And()
            .DoNotHaveNameMatching(@".*Configuration.*")
            .And()
            .DoNotHaveNameMatching(@".*Extension.*")
            .And()
            .DoNotHaveNameMatching(@".*DbContext.*")
            .And()
            .DoNotHaveNameEndingWith("Constants")
            .And()
            .DoNotResideInNamespaceMatching(@".*Migrations.*")
            .Should()
            .NotBePublic()
            .GetResult();

        if (!result.IsSuccessful)
        {
            var failingServices = result.FailingTypeNames ?? new List<string>();
            var detailedMessage =
                $"Infrastructure services should be internal to enforce proper dependency injection. Failing services: {string.Join(", ", failingServices)}";
            result.IsSuccessful.ShouldBeTrue(detailedMessage);
        }
        else
        {
            result.IsSuccessful.ShouldBeTrue(
                "Infrastructure services should be internal to enforce proper dependency injection");
        }
    }

    [Fact]
    public void DbContext_Should_Be_Internal()
    {
        TestResult result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("DbContext")
            .Should()
            .NotBePublic()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue("DbContext should be internal");
    }

    [Fact]
    public void Infrastructure_Should_Only_Expose_Interfaces_And_Extensions()
    {
        var publicTypes = Types.InAssembly(InfrastructureAssembly)
            .That()
            .ArePublic()
            .GetTypes();

        var failingTypes = new List<string>();

        foreach (var type in publicTypes)
        {
            var isAllowed = type.IsInterface ||
                            type.Name.EndsWith("Extensions", StringComparison.InvariantCulture) ||
                            type.Name.EndsWith("Configuration", StringComparison.InvariantCulture) ||
                            type.Name.EndsWith("Constants", StringComparison.InvariantCulture) ||
                            type.Name.Contains("DependencyInjection", StringComparison.InvariantCulture) ||
                            type.Namespace?.Contains("Migrations") == true ||
                            type.IsEnum;

            if (!isAllowed)
            {
                failingTypes.Add($"{type.Name} (should be internal)");
            }
        }

        if (failingTypes.Any())
        {
            var detailedMessage =
                $"Infrastructure should only expose interfaces, extensions, configurations, constants, and enums as public types. Failing types: {string.Join(", ", failingTypes)}";
            failingTypes.ShouldBeEmpty(detailedMessage);
        }
    }

    [Fact]
    public void Infrastructure_Configurations_Should_Be_Internal()
    {
        TestResult result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("Configuration")
            .And()
            .DoNotHaveNameMatching(@".*Extensions.*")
            .Should()
            .NotBePublic()
            .GetResult();

        if (!result.IsSuccessful)
        {
            var failingConfigurations = result.FailingTypeNames ?? new List<string>();
            var detailedMessage =
                $"Entity configurations should be internal. Failing configurations: {string.Join(", ", failingConfigurations)}";
            result.IsSuccessful.ShouldBeTrue(detailedMessage);
        }
        else
        {
            result.IsSuccessful.ShouldBeTrue("Entity configurations should be internal");
        }
    }

    [Fact]
    public void Infrastructure_Should_Implement_Application_Interfaces()
    {
        var applicationInterfaces = Types.InAssembly(ApplicationAssembly)
            .That()
            .AreInterfaces()
            .And()
            .ResideInNamespace("Application.Abstractions")
            .And()
            .DoNotResideInNamespace("Application.Abstractions.Messaging")
            .GetTypes()
            .Where(t => !t.Name.StartsWith('I') || IsValidClassName(t))
            .ToList();

        var infrastructureTypes = Types.InAssembly(InfrastructureAssembly)
            .That()
            .AreClasses()
            .GetTypes();

        foreach (var appInterface in applicationInterfaces)
        {
            var hasImplementation = infrastructureTypes
                .Any(t => t.GetInterfaces().Contains(appInterface));

            if (appInterface.Name.Contains("Repository") ||
                appInterface.Name.Contains("Service") ||
                appInterface.Name.Contains("Provider"))
            {
                hasImplementation.ShouldBeTrue(
                    $"Application interface {appInterface.Name} should have an implementation in Infrastructure");
            }
        }

        return;

        static bool IsValidClassName(Type t) => !t.Name.EndsWith("Command", StringComparison.InvariantCulture) &&
                                                !t.Name.EndsWith("Query", StringComparison.InvariantCulture) &&
                                                !t.Name.EndsWith("Handler", StringComparison.InvariantCulture);
    }
}
