using Infrastructure.Database;
using Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace ArchitectureTests.Infrastructure;

public class PersistenceTests
{
    private const string TimestampWithTimeZone = "timestamp with time zone";

    // xUnit creates a new instance per [Fact], so share via static. Lazy (not a static initializer) rethrows
    // model-building errors as-is instead of wrapping them in TypeInitializationException.
    private static readonly Lazy<List<IProperty>> Properties = new(MappedProperties);

    [Fact]
    public void Entities_Should_Not_Persist_DateTime()
    {
        // DateTime carries no offset: its meaning depends on Kind, which EF does not round-trip.
        var violations = Properties.Value
            .Where(p => UnderlyingType(p) == typeof(DateTime))
            .Select(Describe)
            .ToList();

        violations.ShouldBeEmpty(
            $"Persisted timestamps must be DateTimeOffset, not DateTime. Violations: {string.Join(", ", violations)}");
    }

    [Fact]
    public void DateTimeOffset_Should_Map_To_Timestamp_With_Time_Zone()
    {
        var violations = Properties.Value
            .Where(p => UnderlyingType(p) == typeof(DateTimeOffset) &&
                        p.GetRelationalTypeMapping().StoreType != TimestampWithTimeZone)
            .Select(p => $"{Describe(p)} ({p.GetRelationalTypeMapping().StoreType})")
            .ToList();

        violations.ShouldBeEmpty(
            $"DateTimeOffset columns must be '{TimestampWithTimeZone}'. Violations: {string.Join(", ", violations)}");
    }

    // Builds the real model offline; UseNpgsql never opens the connection.
    private static List<IProperty> MappedProperties()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=unused")
            .Options;

        using var context = new ApplicationDbContext(options, new OutboxSignal());

        return context.Model.GetEntityTypes().SelectMany(e => e.GetProperties()).ToList();
    }

    private static Type UnderlyingType(IProperty property) =>
        Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;

    private static string Describe(IProperty property) =>
        $"{property.DeclaringType.ClrType.Name}.{property.Name}";
}
