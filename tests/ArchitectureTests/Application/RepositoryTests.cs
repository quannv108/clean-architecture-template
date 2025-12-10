using Application.Abstractions.Data;
using Microsoft.Extensions.Caching.Hybrid;
using NetArchTest.Rules;
using SharedKernel;
using Shouldly;

namespace ArchitectureTests.Application;

public class RepositoryTests : BaseTest
{
    [Fact]
    public void Repository_Classes_Should_Be_In_Data_Namespace()
    {
        var repositoryTypes = Types.InAssembly(ApplicationAssembly)
            .That()
            .AreClasses()
            .And()
            .HaveNameEndingWith("Repository")
            .And()
            .DoNotHaveNameEndingWith("CachedRepository")
            .GetTypes();

        var violations = new List<string>();

        foreach (var repositoryType in repositoryTypes)
        {
            if (!repositoryType.Namespace?.Contains(".Data", StringComparison.InvariantCulture) ?? true)
            {
                violations.Add($"{repositoryType.Name} (found in: {repositoryType.Namespace})");
            }
        }

        if (violations.Any())
        {
            var violationMessage =
                $"Repository classes (but not CachedRepository) should be in Data namespace. Found {violations.Count} violation(s):\n{string.Join("\n", violations.Select(v => $"  - {v}"))}";
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
                $"CachedRepository classes should be in Data.Caching namespace. Found {violations.Count} violation(s):\n{string.Join("\n", violations.Select(v => $"  - {v}"))}";
            true.ShouldBeFalse(violationMessage);
        }
    }
}
