using System.Reflection;
using NetArchTest.Rules;
using Shouldly;
using SharedKernel;

namespace ArchitectureTests.Domain;

public class DomainTests : BaseTest
{
    [Fact]
    public void Domain_Entities_Should_Inherit_From_Entity()
    {
        // Get all domain types that should inherit from Entity, manually excluding enums
        var domainTypes = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceMatching(@"Domain\..*")
            .And()
            .AreClasses()
            .And()
            .DoNotHaveNameMatching(@".*Error.*")
            .And()
            .DoNotHaveNameMatching(@".*Event.*")
            .And()
            .DoNotHaveNameMatching(@".*Exception.*")
            .And()
            .AreNotAbstract()
            .And()
            .AreNotInterfaces()
            .And()
            .AreNotStatic()
            .GetTypes()
            .Where(t => !t.IsEnum) // Manually exclude enums
            .Where(t => t.BaseType?.Name != "ValueObject") // Exclude ValueObjects
            .ToList();

        var failingEntities = new List<string>();

        // Check each type for Entity inheritance
        foreach (var type in domainTypes)
        {
            if (!type.IsSubclassOf(typeof(Entity)))
            {
                failingEntities.Add(type.Name);
            }
        }

        if (failingEntities.Any())
        {
            var detailedMessage =
                $"Domain entities should inherit from Entity base class. Failed types: {string.Join(", ", failingEntities)}";
            failingEntities.ShouldBeEmpty(detailedMessage);
        }
    }

    [Fact]
    public void Domain_Should_Not_Have_Infrastructure_Dependencies()
    {
        var forbiddenNamespaces = new[]
        {
            "System.Data",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.Extensions.DependencyInjection",
            "Microsoft.Extensions.Configuration",
            "Microsoft.Extensions.Logging",
            "Microsoft.AspNetCore",
            "System.Security.Cryptography",
            "Serilog"
        };

        foreach (var ns in forbiddenNamespaces)
        {
            TestResult result = Types.InAssembly(DomainAssembly)
                .Should()
                .NotHaveDependencyOn(ns)
                .GetResult();

            result.IsSuccessful.ShouldBeTrue($"Domain should not depend on infrastructure namespace: {ns}");
        }
    }

    [Fact]
    public void Domain_Errors_Should_Be_Static_Classes()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .That()
            .HaveNameEndingWith("Errors")
            .Should()
            .BeStatic()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue("Domain error classes should be static");
    }

    [Fact]
    public void Domain_Should_Not_Have_Public_Setters()
    {
        var domainTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity))
            .GetTypes();

        foreach (var type in domainTypes)
        {
            var publicSetters = type.GetProperties()
                .Where(p => p.SetMethod?.IsPublic == true);

            if (publicSetters.Any())
            {
                publicSetters.ShouldBeEmpty(
                    $"Domain entity {type.Name} should not have public setters. Properties with public setters: {string.Join(", ", publicSetters.Select(p => p.Name))}");
            }
        }
    }
}
