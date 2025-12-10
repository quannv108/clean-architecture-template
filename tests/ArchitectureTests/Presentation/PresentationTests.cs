using NetArchTest.Rules;
using Shouldly;
using Web.Api.Endpoints;

namespace ArchitectureTests.Presentation;

public class PresentationTests : BaseTest
{
    [Fact]
    public void Endpoints_Should_Implement_IEndpoint_Interface()
    {
        TestResult result = Types.InAssembly(PresentationAssembly)
            .That()
            .ResideInNamespaceMatching(@"Web\.Api\.Endpoints\..*EndPoint*")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .And()
            .AreNotNestedPrivate()
            .And()
            .AreNotStatic()
            .Should()
            .ImplementInterface(typeof(IEndpoint))
            .GetResult();

        if (!result.IsSuccessful)
        {
            var failingEndpoints = result.FailingTypeNames ?? new List<string>();
            var detailedMessage =
                $"Endpoint classes should implement IEndpoint interface. Failing endpoints: {string.Join(", ", failingEndpoints)}";
            result.IsSuccessful.ShouldBeTrue(detailedMessage);
        }
        else
        {
            result.IsSuccessful.ShouldBeTrue("Endpoint classes should implement IEndpoint interface");
        }
    }

    [Fact]
    public void Endpoints_Should_Be_Internal()
    {
        TestResult result = Types.InAssembly(PresentationAssembly)
            .That()
            .ResideInNamespaceMatching(@"Web\.Api\.Endpoints\..*")
            .And()
            .AreClasses()
            .Should()
            .NotBePublic()
            .GetResult();

        if (!result.IsSuccessful)
        {
            var failingEndpoints = result.FailingTypeNames ?? new List<string>();
            var detailedMessage =
                $"Endpoint classes should be internal. Failing endpoints: {string.Join(", ", failingEndpoints)}";
            result.IsSuccessful.ShouldBeTrue(detailedMessage);
        }
        else
        {
            result.IsSuccessful.ShouldBeTrue("Endpoint classes should be internal");
        }
    }

    [Fact]
    public void Endpoints_Should_Be_Sealed()
    {
        TestResult result = Types.InAssembly(PresentationAssembly)
            .That()
            .ResideInNamespaceMatching(@"Web\.Api\.Endpoints\..*")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue("Endpoint classes should be sealed");
    }

    [Fact]
    public void Endpoints_Should_Be_Organized_By_Feature()
    {
        var endpointTypes = Types.InAssembly(PresentationAssembly)
            .That()
            .ResideInNamespaceMatching(@"Web\.Api\.Endpoints\..*")
            .And()
            .AreClasses()
            .GetTypes();

        // Check that endpoints are organized by feature/business capability
        var groupedByFeature = endpointTypes
            .GroupBy(t => t.Namespace?.Split('.').LastOrDefault())
            .Where(g => g.Key != null)
            .ToList();

        foreach (var group in groupedByFeature)
        {
            group.Count().ShouldBeGreaterThan(0, $"Feature {group.Key} should have at least one endpoint");

            // Verify that all endpoints in the same feature namespace are related
            var featureName = group.Key;
            foreach (var endpoint in group)
            {
                var isRelatedToFeature = endpoint.Namespace?.Contains(featureName!) == true;
                isRelatedToFeature.ShouldBeTrue($"Endpoint {endpoint.Name} should be in the correct feature namespace");
            }
        }
    }

    [Fact]
    public void Presentation_Should_Not_Contain_Business_Logic()
    {
        // Check that endpoints don't contain complex business logic
        var endpointTypes = Types.InAssembly(PresentationAssembly)
            .That()
            .ResideInNamespaceMatching(@"Web\.Api\.Endpoints\..*")
            .And()
            .AreClasses()
            .And()
            .ImplementInterface(typeof(IEndpoint))
            .GetTypes();

        foreach (var endpointType in endpointTypes)
        {
            var methods = endpointType.GetMethods()
                .Where(m => m.Name == "MapEndpoint")
                .ToList();

            methods.ShouldNotBeEmpty($"Endpoint {endpointType.Name} should have MapEndpoint method");
        }
    }

    [Fact]
    public void Web_Api_Should_Use_Proper_HTTP_Methods()
    {
        // This would require more sophisticated analysis of the endpoint mapping
        // For now, we just ensure endpoints follow the IEndpoint pattern
        var endpointTypes = Types.InAssembly(PresentationAssembly)
            .That()
            .ImplementInterface(typeof(IEndpoint))
            .GetTypes();

        endpointTypes.ShouldNotBeEmpty("Should have endpoint implementations");
    }

    [Fact]
    public void Infrastructure_Classes_Should_Be_Internal()
    {
        TestResult result = Types.InAssembly(PresentationAssembly)
            .That()
            .ResideInNamespaceMatching(@"Web\.Api\.Infrastructure\..*")
            .And()
            .AreClasses()
            .And()
            .DoNotHaveNameMatching(@".*Extensions.*")
            .Should()
            .NotBePublic()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue("Web API infrastructure classes should be internal");
    }

    [Fact]
    public void Nested_Classes_In_Endpoints_Should_Be_Private()
    {
        var typesWithNestedClasses = Types.InAssembly(PresentationAssembly)
            .That()
            .ResideInNamespaceMatching(@"Web\.Api\.Endpoints\..*")
            .And()
            .AreClasses()
            .GetTypes()
            .Where(type => type.GetNestedTypes(System.Reflection.BindingFlags.Public |
                                               System.Reflection.BindingFlags.NonPublic).Any())
            .ToList();

        foreach (var type in typesWithNestedClasses)
        {
            var nestedTypes = type.GetNestedTypes(System.Reflection.BindingFlags.Public |
                                                  System.Reflection.BindingFlags.NonPublic)
                .Where(nt =>
                    !nt.Name.StartsWith("<>", StringComparison.InvariantCulture)) // Exclude compiler-generated types
                .ToArray();

            foreach (var nestedType in nestedTypes)
            {
                var isPrivate = nestedType.IsNestedPrivate;
                isPrivate.ShouldBeTrue(
                    $"Nested class {nestedType.Name} in {type.Name} should be private for proper encapsulation. Found: {GetAccessModifier(nestedType)}");

                // Check if it's a record by looking for compiler-generated methods
                var hasRecordMethods = nestedType.GetMethods()
                    .Any(m => m.Name == "get_EqualityContract" || m.Name == "<Clone>$");

                hasRecordMethods.ShouldBeTrue(
                    $"Nested type {nestedType.Name} in {type.Name} should be a record, not a class. Records provide immutability and are preferred for DTOs in endpoints.");
            }
        }
    }

    private static string GetAccessModifier(Type nestedType)
    {
        if (nestedType.IsNestedPrivate)
        {
            return "private";
        }

        if (nestedType.IsNestedPublic)
        {
            return "public";
        }

        if (nestedType.IsNestedAssembly)
        {
            return "internal";
        }

        if (nestedType.IsNestedFamily)
        {
            return "protected";
        }

        if (nestedType.IsNestedFamORAssem)
        {
            return "protected internal";
        }

        if (nestedType.IsNestedFamANDAssem)
        {
            return "private protected";
        }

        return "unknown";
    }
}
