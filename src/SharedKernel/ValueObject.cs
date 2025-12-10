using System.Collections.Immutable;

namespace SharedKernel;

/// <summary>
/// Value Objects are objects that are defined by their attributes rather than by their identity.
/// Two Value Objects with the same values for all attributes are considered equal, regardless of
/// whether they are the same object in memory.
///
/// Key characteristics of Value Objects:
/// - Immutable: Once created, their state cannot change
/// - Defined by their values: Equality is determined by the values of all properties
/// - No identity: Unlike Entities, they don't have a unique ID that persists over time
/// - Replaceable: If a Value Object changes, you create a new one rather than modifying the existing one
/// - Composable: They can be composed of other Value Objects
///
/// Example usage:
/// <code>
/// public class PrivacyPolicyVersion : ValueObject
/// {
///     public string Version { get; }
///     public DateTime EffectiveDate { get; }
///     public string URL { get; }
///
///     protected override IEnumerable&lt;object&gt; GetAtomicValues()
///     {
///         yield return Version;
///         yield return EffectiveDate;
///         yield return URL;
///     }
/// }
/// </code>
///
/// In this example, two PrivacyPolicyVersion objects with the same Version, EffectiveDate,
/// and URL would be considered equal, even if they are different
/// objects in memory.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Gets the atomic values that define this Value Object.
    /// These values are used for equality comparison and hash code generation.
    /// </summary>
    /// <returns>An enumerable of the atomic values that determine equality for this Value Object.</returns>
    protected abstract IEnumerable<object> GetAtomicValues();

    /// <summary>
    /// Determines whether the specified object is equal to this Value Object.
    /// Two Value Objects are equal if they have the same type and all their atomic values are equal.
    /// </summary>
    /// <param name="obj">The object to compare with this Value Object.</param>
    /// <returns>True if the objects are equal; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        using var thisEnumerator = GetAtomicValues().GetEnumerator();
        using var otherEnumerator = other.GetAtomicValues().GetEnumerator();

        while (thisEnumerator.MoveNext() && otherEnumerator.MoveNext())
        {
            if (thisEnumerator.Current is null)
            {
                if (otherEnumerator.Current is not null)
                {
                    return false;
                }
            }
            else if (!thisEnumerator.Current.Equals(otherEnumerator.Current))
            {
                return false;
            }
        }

        return !thisEnumerator.MoveNext() && !otherEnumerator.MoveNext();
    }

    /// <summary>
    /// Gets the hash code for this Value Object.
    /// The hash code is based on all the atomic values that define this Value Object.
    /// </summary>
    /// <returns>A hash code for this Value Object.</returns>
    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => (x * 397) ^ y);
    }
}
