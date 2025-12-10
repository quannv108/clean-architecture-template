using Application.Abstractions.Data;
using Microsoft.Extensions.Caching.Hybrid;
using NetArchTest.Rules;
using SharedKernel;
using Shouldly;

namespace ArchitectureTests.Application;

public class CachedRepositoryTests : BaseTest
{
    [Fact]
    public void Only_CachedRepository_Classes_Can_Use_HybridCache()
    {
        var typesUsingHybridCache = Types.InAssembly(ApplicationAssembly)
            .That()
            .AreClasses()
            .GetTypes()
            .Where(type => type.GetConstructors()
                .Any(constructor => constructor.GetParameters()
                    .Any(param => param.ParameterType == typeof(HybridCache))))
            .ToList();

        var violations = new List<string>();

        foreach (var type in typesUsingHybridCache)
        {
            var isCachedRepository = type.Name.EndsWith("CachedRepository", StringComparison.InvariantCulture);

            if (!isCachedRepository)
            {
                violations.Add(type.Name);
            }
        }

        if (violations.Any())
        {
            var violationMessage =
                $"Only CachedRepository classes can use HybridCache. Found {violations.Count} violation(s):\n{string.Join("\n", violations.Select(v => $"  - {v}"))}";
            true.ShouldBeFalse(violationMessage);
        }
    }

    [Fact]
    public void CachedRepository_Classes_Should_Be_In_Data_Caching_Namespace()
    {
        var cachedRepositoryTypes = Types.InAssembly(ApplicationAssembly)
            .That()
            .AreClasses()
            .And()
            .HaveNameEndingWith("CachedRepository")
            .GetTypes();

        var violations = new List<string>();

        foreach (var repositoryType in cachedRepositoryTypes)
        {
            if (!repositoryType.Namespace?.Contains(".Data.Caching", StringComparison.InvariantCulture) ?? true)
            {
                violations.Add($"{repositoryType.Name} (found in: {repositoryType.Namespace})");
            }
        }

        if (violations.Any())
        {
            var violationMessage =
                $"CachedRepository classes should be in Data namespace. Found {violations.Count} violation(s):\n{string.Join("\n", violations.Select(v => $"  - {v}"))}";
            true.ShouldBeFalse(violationMessage);
        }
    }

    [Fact]
    public void CachedRepository_Classes_Should_Not_Return_Entity_Types()
    {
        var cachedRepositoryTypes = Types.InAssembly(ApplicationAssembly)
            .That()
            .AreClasses()
            .And()
            .HaveNameEndingWith("CachedRepository")
            .GetTypes();

        var domainEntityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity))
            .GetTypes()
            .ToHashSet();

        var violations = new List<string>();

        foreach (var repositoryType in cachedRepositoryTypes)
        {
            var publicMethods = repositoryType
                .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(m => !m.IsSpecialName && m.DeclaringType == repositoryType);

            foreach (var method in publicMethods)
            {
                var returnType = method.ReturnType;

                // Handle Task<T> and Task<List<T>> return types
                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    returnType = returnType.GetGenericArguments()[0];
                }

                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    returnType = returnType.GetGenericArguments()[0];
                }

                if (domainEntityTypes.Contains(returnType))
                {
                    violations.Add($"{repositoryType.Name}.{method.Name}() returns Entity type {returnType.Name}");
                }
            }
        }

        if (violations.Any())
        {
            var violationMessage =
                $"CachedRepository classes should not return Entity types. Use Response DTOs instead. Found {violations.Count} violation(s):\n{string.Join("\n", violations.Select(v => $"  - {v}"))}";
            true.ShouldBeFalse(violationMessage);
        }
    }

    [Fact]
    public void CachedRepository_Should_Not_Depend_On_DbContext_Types()
    {
        var forbiddenTypes = new[]
        {
            typeof(Microsoft.EntityFrameworkCore.DbContext),
            typeof(IApplicationDbContext)
        };

        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("CachedRepository")
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenTypes.Select(t => t.FullName).ToArray())
            .GetResult();

        if (!result.IsSuccessful)
        {
            var failingTypes = result.FailingTypeNames ?? new List<string>();
            var violationMessage =
                $"CachedRepository should not depend on ApplicationDbContext, DbContext, or IApplicationDbContext. Found {failingTypes.Count} violation(s):\n{string.Join("\n", failingTypes.Select(v => $"  - {v}"))}";
            result.IsSuccessful.ShouldBeTrue(violationMessage);
        }
        else
        {
            result.IsSuccessful.ShouldBeTrue(
                "CachedRepository should not depend on ApplicationDbContext, DbContext, or IApplicationDbContext");
        }
    }
}
