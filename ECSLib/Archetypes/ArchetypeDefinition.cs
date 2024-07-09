using ECSLib.Extensions;

namespace ECSLib.Archetypes;

/// <summary>
/// Represents the definition of an archetype by the component types it contains.
/// </summary>
internal readonly struct ArchetypeDefinition : IEquatable<ArchetypeDefinition>
{
    public IReadOnlySet<Type> Components { get; } = new HashSet<Type>();
    
    public ArchetypeDefinition(IEnumerable<Type> types)
    {
        Components = new HashSet<Type>(types);
    }
    
    #region EQUALITY
    public bool Equals(ArchetypeDefinition other) => Components.SetEquals(other.Components);

    public override bool Equals(object? obj) => obj is ArchetypeDefinition other && Equals(other);

    public override int GetHashCode() => Components.HashContent();

    public static bool operator ==(ArchetypeDefinition left, ArchetypeDefinition right) => left.Equals(right);

    public static bool operator !=(ArchetypeDefinition left, ArchetypeDefinition right) => !(left == right);
    #endregion
}