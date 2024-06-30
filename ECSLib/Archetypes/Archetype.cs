using ECSLib.Extensions;

namespace ECSLib.Archetypes;

internal readonly struct Archetype : IEquatable<Archetype>
{
    private readonly int _hash = 0;
    public IReadOnlySet<Type> Components { get; } = new HashSet<Type>();
    
    public Archetype(IEnumerable<Type> types)
    {
        Components = new HashSet<Type>(types);
        _hash = Components.HashContent();
    }

    public bool Equals(Archetype other) => Components.All(other.Components.Contains);

    public override bool Equals(object? obj) => obj is Archetype other && Equals(other);

    public override int GetHashCode() => _hash;

    public static bool operator ==(Archetype left, Archetype right) => left.Equals(right);

    public static bool operator !=(Archetype left, Archetype right) => !(left == right);
}