namespace ECSLib.Archetypes;

internal readonly struct Archetype : IEquatable<Archetype>
{
    private readonly int _hash;
    public IReadOnlySet<Type> Components { get; }

    public Archetype(IEnumerable<Type> types)
    {
        Components = new HashSet<Type>(types);
        _hash = CalculateHash(Components);
    }

    public static int CalculateHash(IEnumerable<Type> types)
    {
        HashCode hash = new();
        foreach (var type in types)
        {
            hash.Add(type);
        }
        return hash.ToHashCode();
    }

    public bool Equals(Archetype other) => Components.SequenceEqual(other.Components);

    public override bool Equals(object? obj) => obj is Archetype other && Equals(other);

    public override int GetHashCode() => _hash;

    public static bool operator ==(Archetype left, Archetype right) => left.Equals(right);

    public static bool operator !=(Archetype left, Archetype right) => !(left == right);
}