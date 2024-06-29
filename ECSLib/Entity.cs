namespace ECSLib;

public struct Entity: IEquatable<Entity>
{
    public int ID { get; }
    
    public bool Equals(Entity other) => ID == other.ID;

    public override bool Equals(object? obj) => obj is Entity other && Equals(other);

    public override int GetHashCode() => ID;

    public static bool operator ==(Entity left, Entity right) => left.Equals(right);

    public static bool operator !=(Entity left, Entity right) => !(left == right);
}