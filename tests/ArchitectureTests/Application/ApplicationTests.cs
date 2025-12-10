using Application.Abstractions.Messaging;
using NetArchTest.Rules;
using Shouldly;
using SharedKernel;

namespace ArchitectureTests.Application;

public class ApplicationTests : BaseTest
{
    [Fact]
    public void Commands_Should_Implement_ICommand_Interface()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Command")
            .And()
            .AreNotInterfaces()
            .Should()
            .ImplementInterface(typeof(ICommand))
            .Or()
            .ImplementInterface(typeof(ICommand<>))
            .GetResult();

        if (!result.IsSuccessful)
        {
            var failingCommands = result.FailingTypeNames ?? new List<string>();
            var detailedMessage =
                $"Commands should implement ICommand interface. Failing commands: {string.Join(", ", failingCommands)}";
            result.IsSuccessful.ShouldBeTrue(detailedMessage);
        }
        else
        {
            result.IsSuccessful.ShouldBeTrue("Commands should implement ICommand interface");
        }
    }

    [Fact]
    public void Queries_Should_Implement_IQuery_Interface()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Query")
            .Should()
            .ImplementInterface(typeof(IQuery<>))
            .GetResult();

        if (!result.IsSuccessful)
        {
            var failingQueries = result.FailingTypeNames ?? new List<string>();
            var detailedMessage =
                $"Queries should implement IQuery interface. Failing queries: {string.Join(", ", failingQueries)}";
            result.IsSuccessful.ShouldBeTrue(detailedMessage);
        }
        else
        {
            result.IsSuccessful.ShouldBeTrue("Queries should implement IQuery interface");
        }
    }


    [Fact]
    public void Queries_Should_Be_Records()
    {
        var queryTypes = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Query")
            .GetTypes();

        foreach (var type in queryTypes)
        {
            type.IsValueType.ShouldBeFalse($"Query {type.Name} should be a record, not a value type");

            // Check if it's a record by looking for compiler-generated methods
            var hasRecordMethods = type.GetMethods()
                .Any(m => m.Name == "get_EqualityContract" || m.Name == "<Clone>$");

            hasRecordMethods.ShouldBeTrue($"Query {type.Name} should be a record type");
        }
    }

    [Fact]
    public void Handlers_Should_Return_Result_Type()
    {
        var handlerTypes = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .GetTypes();

        foreach (var handlerType in handlerTypes)
        {
            var handleMethods = handlerType.GetMethods()
                .Where(m => m.Name == "Handle")
                .ToList();

            foreach (var method in handleMethods)
            {
                var returnType = method.ReturnType;

                // Check if it's Task<Result> or Task<Result<T>>
                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var taskArgument = returnType.GetGenericArguments()[0];
                    var isResultType = false;
                    if (taskArgument == typeof(Result))
                    {
                        isResultType = true;
                    }
                    else if (taskArgument.IsGenericType && taskArgument.GetGenericTypeDefinition() == typeof(Result<>))
                    {
                        isResultType = true;
                    }

                    isResultType.ShouldBeTrue(
                        $"Handler {handlerType.Name}.Handle should return Task<Result> or Task<Result<T>>, but returns {returnType.Name}");
                }
            }
        }
    }

    [Fact]
    public void Application_Should_Not_Reference_Infrastructure_Implementations()
    {
        var forbiddenNamespaces = new[]
        {
            "System.Data.SqlClient",
            "Npgsql",
            "Microsoft.Extensions.Configuration",
            "Serilog"
        };

        foreach (var ns in forbiddenNamespaces)
        {
            TestResult result = Types.InAssembly(ApplicationAssembly)
                .Should()
                .NotHaveDependencyOn(ns)
                .GetResult();

            result.IsSuccessful.ShouldBeTrue($"Application should not depend on infrastructure namespace: {ns}");
        }
    }

    [Fact]
    public void Application_Should_Have_Organized_Vertical_Slices()
    {
        var applicationTypes = Types.InAssembly(ApplicationAssembly)
            .That()
            .AreClasses()
            .GetTypes()
            .ToList();

        // Check that types are organized by feature/business capability
        var groupedByNamespace = applicationTypes
            .GroupBy(t => t.Namespace)
            .Where(g => g.Key != null && !g.Key.EndsWith(".Abstractions", StringComparison.InvariantCulture))
            .ToList();

        foreach (var group in groupedByNamespace)
        {
            var hasCommand = group.Any(t => t.Name.EndsWith("Command", StringComparison.InvariantCulture));
            var hasQuery = group.Any(t => t.Name.EndsWith("Query", StringComparison.InvariantCulture));
            var hasHandler = group.Any(t => t.Name.EndsWith("Handler", StringComparison.InvariantCulture));

            if (hasCommand || hasQuery)
            {
                hasHandler.ShouldBeTrue(
                    $"Namespace {group.Key} contains commands/queries but no handlers. Each vertical slice should be self-contained.");
            }
        }
    }

    [Fact]
    public void Application_Namespaces_Should_Only_Use_Corresponding_Domain_Namespaces()
    {
        var violations = new List<string>();

        // Get all Application types (excluding abstractions)
        var applicationTypes = Types.InAssembly(ApplicationAssembly)
            .That()
            .AreClasses()
            .And()
            .DoNotResideInNamespace("Application.Abstractions")
            .GetTypes();

        // Get all Domain namespaces to build the forbidden list dynamically
        var allDomainNamespaces = Types.InAssembly(DomainAssembly)
            .That()
            .AreClasses()
            .GetTypes()
            .Where(t => !string.IsNullOrEmpty(t.Namespace))
            .Select(t => t.Namespace!)
            .Where(ns => ns.StartsWith("Domain.", StringComparison.InvariantCulture))
            .Select(ns => ns.Split('.').Take(2).Aggregate((a, b) => $"{a}.{b}")) // Get Domain.FeatureName
            .Distinct()
            .ToList();

        // Group Application types by their feature namespace
        var applicationFeatureGroups = applicationTypes
            .Where(t => t.Namespace != null &&
                        t.Namespace.StartsWith("Application.", StringComparison.InvariantCulture))
            .GroupBy(t =>
                t.Namespace!.Split('.').Take(2).Aggregate((a, b) => $"{a}.{b}")) // Get Application.FeatureName
            .ToList();

        foreach (var featureGroup in applicationFeatureGroups)
        {
            var applicationFeatureNamespace = featureGroup.Key;
            var featureName = applicationFeatureNamespace.Replace("Application.", "");
            var expectedDomainNamespace = $"Domain.{featureName}";

            // Build forbidden namespaces: all Domain namespaces except the corresponding one and Domain.Users
            var forbiddenNamespaces = allDomainNamespaces
                .Where(ns => ns != expectedDomainNamespace
                             && ns != "Domain.Users" && ns != "Domain.Roles" && ns != "Domain.Profiles"
                             && ns != "Domain")
                .ToArray();

            // Check each forbidden namespace
            foreach (var forbiddenNamespace in forbiddenNamespaces)
            {
                var result = Types.InAssembly(ApplicationAssembly)
                    .That()
                    .ResideInNamespace(applicationFeatureNamespace)
                    .Should()
                    .NotHaveDependencyOn(forbiddenNamespace)
                    .GetResult();

                if (!result.IsSuccessful)
                {
                    var failingTypes = result.FailingTypeNames ?? new List<string>();
                    foreach (var failingType in failingTypes)
                    {
                        // Check for exceptional cases that are allowed
                        if (IsAllowedException(failingType, forbiddenNamespace))
                        {
                            continue; // Skip this violation as it's an allowed exception
                        }

                        violations.Add(
                            $"Application type {failingType} in namespace {applicationFeatureNamespace} " +
                            $"should only reference Domain types from {expectedDomainNamespace} namespace " +
                            $"or Domain.Users or Domain.Roles or Domain.Profiles namespace. " +
                            $"Found reference to {forbiddenNamespace} namespace.");
                    }
                }
            }
        }

        if (violations.Count > 0)
        {
            var message = $"Found {violations.Count} namespace violations:\n{string.Join("\n", violations)}";
            violations.Count.ShouldBe(0, message);
        }
    }

    private static bool IsAllowedException(string typeName, string forbiddenNamespace)
    {
        // Define exceptional cases where cross-domain dependencies are allowed
        return (typeName, forbiddenNamespace) switch
        {
            // Default: not an exception
            _ => false
        };
    }

    [Fact]
    public void Commands_Should_Be_Records()
    {
        var commandTypes = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Command")
            .And()
            .AreNotInterfaces()
            .GetTypes();

        var failingCommands = new List<string>();
        foreach (var type in commandTypes)
        {
            // Check if it's a record by looking for compiler-generated methods
            var hasRecordMethods = type.GetMethods()
                .Any(m => m.Name == "get_EqualityContract" || m.Name == "<Clone>$");

            if (!hasRecordMethods)
            {
                failingCommands.Add(type.FullName ?? type.Name);
            }
        }

        var detailedMessage =
            $"Commands should be records, not regular classes. Failing commands: {string.Join(", ", failingCommands)}";
        failingCommands.Count.ShouldBe(0, detailedMessage);
    }

    // [Fact]
    // public void Application_Slices_Should_Not_Have_Cross_Slice_Dependencies()
    // {
    //     var violations = new List<string>();
    //
    //     // Get all Application types (excluding shared abstractions and infrastructure)
    //     var applicationTypes = Types.InAssembly(ApplicationAssembly)
    //         .That()
    //         .AreClasses()
    //         .And()
    //         .DoNotResideInNamespace("Application.Abstractions")
    //         .GetTypes()
    //         .ToList(); // Materialize to avoid multiple enumerations
    //
    //     // Get all Application namespaces to build the forbidden list dynamically
    //     var allApplicationNamespaces = applicationTypes
    //         .Where(t => !string.IsNullOrEmpty(t.Namespace))
    //         .Select(t => t.Namespace!)
    //         .Where(ns => ns.StartsWith("Application.", StringComparison.InvariantCulture))
    //         .Select(ns =>
    //         {
    //             var parts = ns.Split('.');
    //             return parts.Length >= 2 ? $"{parts[0]}.{parts[1]}" : ns;
    //         })
    //         .Distinct()
    //         .ToList();
    //
    //     // Group Application types by their feature namespace
    //     var applicationFeatureGroups = applicationTypes
    //         .Where(t => t.Namespace != null &&
    //                     t.Namespace.StartsWith("Application.", StringComparison.InvariantCulture))
    //         .GroupBy(t =>
    //         {
    //             var parts = t.Namespace!.Split('.');
    //             return parts.Length >= 2 ? $"{parts[0]}.{parts[1]}" : t.Namespace!;
    //         })
    //         .ToList();
    //
    //     foreach (var featureGroup in applicationFeatureGroups)
    //     {
    //         var applicationFeatureNamespace = featureGroup.Key;
    //
    //         // Build forbidden namespaces: all Application namespaces except:
    //         // 1. The current slice itself
    //         // 2. Shared infrastructure (Abstractions, Outbox)
    //         var forbiddenNamespaces = allApplicationNamespaces
    //             .Where(ns => ns != applicationFeatureNamespace
    //                          && ns != "Application.Abstractions"
    //                          && ns != "Application.Outbox"
    //                          && ns != "Application")
    //             .ToArray();
    //
    //         // Check each forbidden namespace - STRICT ISOLATION
    //         foreach (var forbiddenNamespace in forbiddenNamespaces)
    //         {
    //             var result = Types.InAssembly(ApplicationAssembly)
    //                 .That()
    //                 .ResideInNamespace(applicationFeatureNamespace)
    //                 .Should()
    //                 .NotHaveDependencyOn(forbiddenNamespace)
    //                 .GetResult();
    //
    //             if (!result.IsSuccessful)
    //             {
    //                 var failingTypes = result.FailingTypeNames ?? new List<string>();
    //                 foreach (var failingType in failingTypes)
    //                 {
    //                     violations.Add(
    //                         $"Application type {failingType} in namespace {applicationFeatureNamespace} " +
    //                         $"should not reference other Application slices. " +
    //                         $"Found reference to {forbiddenNamespace} namespace. " +
    //                         $"Each vertical slice should be self-contained and isolated.");
    //                 }
    //             }
    //         }
    //     }
    //
    //     if (violations.Count > 0)
    //     {
    //         var message = $"Found {violations.Count} cross-slice dependency violations:\n{string.Join("\n", violations)}";
    //         violations.Count.ShouldBe(0, message);
    //     }
    // }
}
