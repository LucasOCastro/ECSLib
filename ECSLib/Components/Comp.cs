namespace ECSLib.Components;

/// <summary> Represents a reference to a component which may have a value or not. </summary>
public readonly ref struct Comp<T> where T: struct
{
    public readonly ref T Value;
    public readonly bool HasValue;

    public Comp()
    {
        HasValue = false;
    }

    public Comp(ref T value)
    {
        Value = ref value;
        HasValue = true;
    }
}