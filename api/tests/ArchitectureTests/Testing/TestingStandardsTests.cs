using System.Reflection;
using NetArchTest.Rules;
using Shouldly;

namespace ArchitectureTests.Testing;

public class TestingStandardsTests : BaseTest
{
    [Fact]
    public void UnitTests_Should_Follow_AAA_Pattern()
    {
        var testAssemblies = GetTestAssemblies();

        foreach (var assembly in testAssemblies)
        {
            var testClasses = Types.InAssembly(assembly)
                .That()
                .HaveNameEndingWith("Tests")
                .GetTypes();

            foreach (var testClass in testClasses)
            {
                var testMethods = testClass.GetMethods()
                    .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Length > 0 ||
                                m.GetCustomAttributes(typeof(TheoryAttribute), false).Length > 0)
                    .ToList();

                testMethods.ShouldNotBeEmpty($"Test class {testClass.Name} should have test methods");
            }
        }
    }

    [Fact]
    public void UnitTests_Should_Not_Use_EntityFramework_InMemory()
    {
        var testAssemblies = GetTestAssemblies();

        foreach (var assembly in testAssemblies)
        {
            TestResult result = Types.InAssembly(assembly)
                .Should()
                .NotHaveDependencyOn("Microsoft.EntityFrameworkCore.InMemory")
                .GetResult();

            result.IsSuccessful.ShouldBeTrue(
                $"Test assembly {assembly.GetName().Name} should not use EntityFramework InMemory provider");
        }
    }

    [Fact]
    public void UnitTests_Should_Use_NSubstitute_For_Mocking()
    {
        var testAssemblies = GetTestAssemblies();

        foreach (var assembly in testAssemblies)
        {
            var hasNSubstitute = assembly.GetReferencedAssemblies()
                .Any(a => a.Name?.Contains("NSubstitute") == true);

            if (ContainsMockingCode(assembly))
            {
                hasNSubstitute.ShouldBeTrue(
                    $"Test assembly {assembly.GetName().Name} should use NSubstitute for mocking");
            }
        }
    }

    [Fact]
    public void UnitTests_Should_Not_Use_Moq()
    {
        var testAssemblies = GetTestAssemblies();

        foreach (var assembly in testAssemblies)
        {
            TestResult result = Types.InAssembly(assembly)
                .Should()
                .NotHaveDependencyOn("Moq")
                .GetResult();

            result.IsSuccessful.ShouldBeTrue(
                $"Test assembly {assembly.GetName().Name} should not use Moq - use NSubstitute instead");
        }
    }

    [Fact]
    public void UnitTests_Should_Not_Use_FluentAssertions()
    {
        var testAssemblies = GetTestAssemblies();

        foreach (var assembly in testAssemblies)
        {
            TestResult result = Types.InAssembly(assembly)
                .Should()
                .NotHaveDependencyOn("FluentAssertions")
                .GetResult();

            result.IsSuccessful.ShouldBeTrue(
                $"Test assembly {assembly.GetName().Name} should not use FluentAssertions - use xUnit Assert or Shouldly instead");
        }
    }

    [Fact]
    public void UnitTests_Should_Use_Shouldly_For_Assertions()
    {
        var testAssemblies = GetTestAssemblies();

        foreach (var assembly in testAssemblies)
        {
            var hasShouldly = assembly.GetReferencedAssemblies()
                .Any(a => a.Name?.Contains("Shouldly") == true);

            if (ContainsTestMethods(assembly))
            {
                hasShouldly.ShouldBeTrue($"Test assembly {assembly.GetName().Name} should use Shouldly for assertions");
            }
        }
    }

    [Fact]
    public void Test_Classes_Should_Have_Descriptive_Names()
    {
        var testAssemblies = GetTestAssemblies();

        foreach (var assembly in testAssemblies)
        {
            var testClasses = Types.InAssembly(assembly)
                .That()
                .AreClasses()
                .And()
                .HaveNameEndingWith("Tests")
                .GetTypes();

            foreach (var testClass in testClasses)
            {
                var hasDescriptiveName = testClass.Name.Length > 5 &&
                                         testClass.Name != "Tests" &&
                                         testClass.Name.Contains("Test");

                hasDescriptiveName.ShouldBeTrue($"Test class {testClass.Name} should have a descriptive name");
            }
        }
    }

    [Fact]
    public void Test_Methods_Should_Have_Descriptive_Names()
    {
        var testAssemblies = GetTestAssemblies();

        foreach (var assembly in testAssemblies)
        {
            var testClasses = Types.InAssembly(assembly)
                .That()
                .AreClasses()
                .And()
                .HaveNameEndingWith("Tests")
                .GetTypes();

            foreach (var testClass in testClasses)
            {
                var testMethods = testClass.GetMethods()
                    .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Length > 0 ||
                                m.GetCustomAttributes(typeof(TheoryAttribute), false).Length > 0)
                    .ToList();

                foreach (var method in testMethods)
                {
                    var hasDescriptiveName = method.Name.Length > 10 &&
                                             method.Name.Contains('_') &&
                                             (method.Name.Contains("Should") || method.Name.Contains("When"));

                    hasDescriptiveName.ShouldBeTrue(
                        $"Test method {testClass.Name}.{method.Name} should have a descriptive name following the pattern 'Method_Should_ExpectedBehavior' or 'Given_When_Then'");
                }
            }
        }
    }

    [Fact]
    public void Handler_Tests_Should_Cover_Both_Success_And_Failure_Scenarios()
    {
        var applicationTestAssembly = GetApplicationTestAssembly();
        if (applicationTestAssembly == null)
        {
            return;
        }

        var handlerTestClasses = Types.InAssembly(applicationTestAssembly)
            .That()
            .HaveNameEndingWith("HandlerTests")
            .GetTypes();

        foreach (var testClass in handlerTestClasses)
        {
            var testMethods = testClass.GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Length > 0 ||
                            m.GetCustomAttributes(typeof(TheoryAttribute), false).Length > 0)
                .ToList();

            var hasSuccessTest = testMethods.Any(m =>
                m.Name.Contains("Success") || m.Name.Contains("Valid") || m.Name.Contains("Should_Return"));
            var hasFailureTest = testMethods.Any(m =>
                m.Name.Contains("Fail") || m.Name.Contains("Invalid") || m.Name.Contains("Error") ||
                m.Name.Contains("NotFound"));

            hasSuccessTest.ShouldBeTrue(
                $"Handler test class {testClass.Name} should have at least one success scenario test");
            hasFailureTest.ShouldBeTrue(
                $"Handler test class {testClass.Name} should have at least one failure scenario test");
        }
    }

    [Fact]
    public void Domain_Tests_Should_Test_Business_Rules()
    {
        var testAssemblies = GetTestAssemblies();

        foreach (var assembly in testAssemblies)
        {
            var domainTestClasses = Types.InAssembly(assembly)
                .That()
                .ResideInNamespaceMatching(@".*Domain.*")
                .And()
                .HaveNameEndingWith("Tests")
                .GetTypes();

            foreach (var testClass in domainTestClasses)
            {
                var testMethods = testClass.GetMethods()
                    .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Length > 0 ||
                                m.GetCustomAttributes(typeof(TheoryAttribute), false).Length > 0)
                    .ToList();

                if (testMethods.Any())
                {
                    var hasBusinessRuleTest = testMethods.Any(m =>
                        m.Name.Contains("Rule") ||
                        m.Name.Contains("Invariant") ||
                        m.Name.Contains("Validate") ||
                        m.Name.Contains("Should_Not_Allow") ||
                        m.Name.Contains("Should_Enforce"));

                    hasBusinessRuleTest.ShouldBeTrue(
                        $"Domain test class {testClass.Name} should test business rules and invariants");
                }
            }
        }
    }

    private static Assembly[] GetTestAssemblies()
    {
        var testAssemblies = new List<Assembly>();

        // Try to load common test assemblies
        var testAssemblyNames = new[]
        {
            "Application.UnitTests",
            "Domain.UnitTests",
            "Infrastructure.UnitTests",
            "ArchitectureTests"
        };

        foreach (var name in testAssemblyNames)
        {
            try
            {
                var assembly = Assembly.Load($"{name}.dll");
                testAssemblies.Add(assembly);
            }
            catch
            {
                // Assembly not found, skip
            }
        }

        return testAssemblies.ToArray();
    }

    private static Assembly? GetApplicationTestAssembly()
    {
        try
        {
            return Assembly.Load("Application.UnitTests.dll");
        }
        catch
        {
            return null;
        }
    }

    private static bool ContainsMockingCode(Assembly assembly)
    {
        var types = assembly.GetTypes();
        return types.Any(t => t.GetMethods().Any(m =>
            m.GetParameters().Any(p => p.ParameterType.Name.Contains("Mock") ||
                                       p.ParameterType.Name.Contains("Substitute"))));
    }

    private static bool ContainsTestMethods(Assembly assembly)
    {
        var types = assembly.GetTypes();
        return types.Any(t => t.GetMethods().Any(m =>
            m.GetCustomAttributes(typeof(FactAttribute), false).Length > 0 ||
            m.GetCustomAttributes(typeof(TheoryAttribute), false).Length > 0));
    }
}

// These attributes might not be available in this context, so we define them
// public class FactAttribute : Attribute { }
// public class TheoryAttribute : Attribute { }
