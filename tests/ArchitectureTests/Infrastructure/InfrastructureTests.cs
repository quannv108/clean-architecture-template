using Infrastructure.Database.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NetArchTest.Rules;
using SharedKernel;
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
    public void EntityConfigurations_Should_Reside_In_Database_Configuration_Folder()
    {
        // EF Core entity configurations must live in Infrastructure/Database/Configuration/<Feature>/
        // not in Infrastructure/<Feature>/ or any other sub-path.
        TestResult result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .ImplementInterface(typeof(IEntityTypeConfiguration<>))
            .Should()
            .ResideInNamespaceMatching(@"^Infrastructure\.Database\.Configuration\.")
            .GetResult();

        if (!result.IsSuccessful)
        {
            var failing = result.FailingTypeNames ?? new List<string>();
            result.IsSuccessful.ShouldBeTrue(
                $"Entity configurations must reside in Infrastructure/Database/Configuration/<Feature>/, " +
                $"not in Infrastructure/<Feature>/ or other paths. " +
                $"Failing: {string.Join(", ", failing)}");
        }
    }

    [Fact]
    public void Seeders_Should_Reside_In_Database_Seeder_Folder()
    {
        // Feature-specific seeders (IEntitySeeder<T> implementations) must live in
        // Infrastructure/Database/Seeder/<Feature>/, not in Infrastructure/<Feature>/ or other paths.
        TestResult result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .ImplementInterface(typeof(IEntitySeeder<>))
            .And()
            .AreClasses()
            .Should()
            .ResideInNamespaceMatching(@"^Infrastructure\.Database\.Seeder\.")
            .GetResult();

        if (!result.IsSuccessful)
        {
            var failing = result.FailingTypeNames ?? new List<string>();
            result.IsSuccessful.ShouldBeTrue(
                $"Seeder classes must reside in Infrastructure/Database/Seeder/<Feature>/, " +
                $"not in Infrastructure/<Feature>/ or other paths. " +
                $"Failing: {string.Join(", ", failing)}");
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

    [Fact]
    public void HostedServices_Should_Reside_Only_In_Infrastructure_Layer()
    {
        // IHostedService / BackgroundService are infrastructure concerns — polling, queue processing, scheduling.
        // They must never appear in Domain, Application, or Web.Api.
        var forbiddenAssemblies = new[]
        {
            (Assembly: DomainAssembly, Name: "Domain"),
            (Assembly: ApplicationAssembly, Name: "Application"),
            (Assembly: PresentationAssembly, Name: "Web.Api"),
        };

        var violations = new List<string>();

        foreach (var (assembly, name) in forbiddenAssemblies)
        {
            var found = Types.InAssembly(assembly)
                .That()
                .ImplementInterface(typeof(IHostedService))
                .Or()
                .Inherit(typeof(BackgroundService))
                .GetTypes()
                .Select(t => $"{t.FullName ?? t.Name} (in {name})")
                .ToList();

            violations.AddRange(found);
        }

        violations.ShouldBeEmpty(
            $"Hosted services must reside in Infrastructure, not in other layers. " +
            $"Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void HostedServices_Should_Be_Registered_Via_AddHostedService_In_Infrastructure_DependencyInjection()
    {
        // Every IHostedService implementation in Infrastructure must be registered via
        // AddHostedService<T>() in an Infrastructure DependencyInjection.cs file.
        // This test prevents hosted services from being silently forgotten in DI registration.
        var hostedServiceTypes = Types.InAssembly(InfrastructureAssembly)
            .That()
            .ImplementInterface(typeof(IHostedService))
            .Or()
            .Inherit(typeof(BackgroundService))
            .GetTypes()
            .ToList();

        if (hostedServiceTypes.Count == 0)
        {
            return; // Nothing to validate
        }

        // Walk up from the test output directory to find the repo root (contains src/Infrastructure)
        var searchDir = new DirectoryInfo(AppContext.BaseDirectory);
        DirectoryInfo? infraSourceDir = null;

        while (searchDir is not null)
        {
            var candidate = Path.Combine(searchDir.FullName, "src", "Infrastructure");
            if (Directory.Exists(candidate))
            {
                infraSourceDir = new DirectoryInfo(candidate);
                break;
            }

            searchDir = searchDir.Parent;
        }

        infraSourceDir.ShouldNotBeNull(
            "Could not find src/Infrastructure directory by walking up from the test output path");

        var diFiles = infraSourceDir!.GetFiles("DependencyInjection.cs", SearchOption.AllDirectories);
        diFiles.ShouldNotBeEmpty("Expected at least one DependencyInjection.cs file in Infrastructure");

        var diContent = string.Join("\n", diFiles.Select(f => File.ReadAllText(f.FullName)));

        var notRegistered = hostedServiceTypes
            .Where(t => !diContent.Contains($"AddHostedService<{t.Name}>"))
            .Select(t => t.Name)
            .ToList();

        notRegistered.ShouldBeEmpty(
            $"The following IHostedService implementations are not registered via " +
            $"AddHostedService<T>() in any Infrastructure DependencyInjection.cs file. " +
            $"Missing: {string.Join(", ", notRegistered)}");
    }

    [Fact]
    public void Every_Domain_Entity_Should_Have_Configuration()
    {
        // Get all domain entities (classes inheriting from Entity)
        var domainEntities = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity))
            .GetTypes()
            .ToList();

        // Get all configuration types implementing IEntityTypeConfiguration<T>
        var configurationTypes = Types.InAssembly(InfrastructureAssembly)
            .That()
            .ImplementInterface(typeof(IEntityTypeConfiguration<>))
            .GetTypes()
            .ToList();

        // Extract the entity types that have configurations
        var configuredEntityTypes = configurationTypes
            .SelectMany(configType => configType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                .Select(i => i.GetGenericArguments()[0]))
            .ToHashSet();

        // Find entities without configurations
        var entitiesWithoutConfiguration = domainEntities
            .Where(entity => !configuredEntityTypes.Contains(entity))
            .Select(entity => entity.FullName)
            .ToList();

        entitiesWithoutConfiguration.ShouldBeEmpty(
            $"Every domain entity must have a corresponding IEntityTypeConfiguration<T> in Infrastructure. " +
            $"Missing configurations for: {string.Join(", ", entitiesWithoutConfiguration)}");
    }
}
