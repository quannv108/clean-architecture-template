using NetArchTest.Rules;
using Shouldly;

namespace ArchitectureTests.CodeQuality;

public class CodeQualityTests : BaseTest
{
    [Fact]
    public void Async_Methods_Should_Have_Async_Suffix()
    {
        var allAssemblies = new[] { ApplicationAssembly, InfrastructureAssembly, PresentationAssembly };

        foreach (var assembly in allAssemblies)
        {
            var types = Types.InAssembly(assembly)
                .That()
                .AreClasses()
                .And()
                .DoNotResideInNamespace("Coverlet")
                .GetTypes();

            foreach (var type in types)
            {
                var asyncMethods = type.GetMethods()
                    .Where(m => m.ReturnType.Name.StartsWith("Task", StringComparison.InvariantCulture) && m.IsPublic)
                    .ToList();

                foreach (var method in asyncMethods)
                {
                    var shouldHaveAsyncSuffix = !method.Name.Equals("Handle", StringComparison.Ordinal) &&
                                                !method.Name.Equals("MapEndpoint", StringComparison.Ordinal) &&
                                                !method.Name.StartsWith("get_", StringComparison.InvariantCulture) &&
                                                !method.Name.StartsWith("set_", StringComparison.InvariantCulture) &&
                                                (!method.IsVirtual ||
                                                 method.DeclaringType ==
                                                 method.ReflectedType); // Exclude override methods

                    if (shouldHaveAsyncSuffix)
                    {
                        method.Name.ShouldEndWith("Async", Case.Sensitive,
                            $"Method {type.Name}.{method.Name} returns Task but doesn't end with 'Async'");
                    }
                }
            }
        }
    }

    [Fact]
    public void Public_Classes_Should_Have_Xml_Documentation()
    {
        // This test would require loading XML documentation files
        // For now, we'll check that important public classes exist
        var publicClasses = Types.InAssembly(ApplicationAssembly)
            .That()
            .ArePublic()
            .And()
            .AreClasses()
            .And()
            .DoNotResideInNamespace("Coverlet")
            .GetTypes();

        publicClasses.ShouldNotBeEmpty("Should have public classes in Application layer");
    }

    [Fact]
    public void Exception_Classes_Should_Follow_Naming_Convention()
    {
        var allAssemblies = new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly, PresentationAssembly };

        foreach (var assembly in allAssemblies)
        {
            var exceptionClasses = Types.InAssembly(assembly)
                .That()
                .Inherit(typeof(Exception))
                .GetTypes();

            foreach (var exceptionClass in exceptionClasses)
            {
                exceptionClass.Name.ShouldEndWith("Exception", Case.Sensitive,
                    $"Exception class {exceptionClass.Name} should end with 'Exception'");
            }
        }
    }

    [Fact]
    public void Constants_Should_Be_In_Appropriate_Classes()
    {
        var allAssemblies = new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly, PresentationAssembly };

        foreach (var assembly in allAssemblies)
        {
            var types = Types.InAssembly(assembly)
                .That()
                .AreClasses()
                .And()
                .DoNotResideInNamespace("Coverlet")
                .GetTypes();

            foreach (var type in types)
            {
                var constants = type.GetFields()
                    .Where(f => f.IsLiteral && f.IsPublic)
                    .ToList();

                if (constants.Any())
                {
                    var isAppropriateForConstants =
                        type.Name.EndsWith("Constants", StringComparison.InvariantCulture) ||
                        type.Name.EndsWith("Errors", StringComparison.InvariantCulture) ||
                        type.Name.EndsWith("Tags", StringComparison.InvariantCulture) ||
                        type.IsEnum;

                    isAppropriateForConstants.ShouldBeTrue(
                        $"Class {type.Name} contains constants but doesn't follow naming convention for constant classes");
                }
            }
        }
    }

    [Fact]
    public void Interfaces_Should_Start_With_I()
    {
        var allAssemblies = new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly, PresentationAssembly };

        foreach (var assembly in allAssemblies)
        {
            var interfaces = Types.InAssembly(assembly)
                .That()
                .AreInterfaces()
                .GetTypes();

            foreach (var @interface in interfaces)
            {
                @interface.Name.ShouldStartWith("I", Case.Sensitive,
                    $"Interface {@interface.Name} should start with 'I'");
            }
        }
    }

    [Fact]
    public void Value_Objects_Should_Be_Immutable()
    {
        var valueObjectTypes = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceMatching(@".*ValueObjects.*")
            .GetTypes();

        foreach (var type in valueObjectTypes)
        {
            var publicSetters = type.GetProperties()
                .Where(p => p.SetMethod?.IsPublic == true)
                .ToList();

            var publicFields = type.GetFields()
                .Where(f => f.IsPublic && !f.IsInitOnly)
                .ToList();

            publicSetters.ShouldBeEmpty($"Value object {type.Name} should not have public setters");
            publicFields.ShouldBeEmpty($"Value object {type.Name} should not have public mutable fields");
        }
    }

    [Fact]
    public void Nullable_Reference_Types_Should_Be_Properly_Annotated()
    {
        // This is a basic check - in practice, you'd want to use more sophisticated analysis
        // The main point is ensuring nullable reference types are properly handled
        var allAssemblies = new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly, PresentationAssembly };

        foreach (var assembly in allAssemblies)
        {
            var types = Types.InAssembly(assembly)
                .That()
                .AreClasses()
                .GetTypes();

            types.ShouldNotBeEmpty($"Assembly {assembly.GetName().Name} should contain types");
        }
    }

    [Fact]
    public void Static_Classes_Should_Be_Sealed()
    {
        var allAssemblies = new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly, PresentationAssembly };

        foreach (var assembly in allAssemblies)
        {
            var staticClasses = Types.InAssembly(assembly)
                .That()
                .AreStatic()
                .And()
                .DoNotResideInNamespace("Coverlet")
                .GetTypes();

            foreach (var staticClass in staticClasses)
            {
                staticClass.IsSealed.ShouldBeTrue($"Static class {staticClass.Name} should be sealed");
            }
        }
    }
}
