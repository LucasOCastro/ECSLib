namespace ECSLib.Components.Interning;

public readonly struct PooledRef<T> where T: class?
{
    private readonly int _id;
    
    public PooledRef(T value)
    {
        _id = RefPool<T>.Register(value);
    }

    public T Value
    {
        get => RefPool<T>.Get(_id);
        set => RefPool<T>.Set(_id, value);
    }

    public static implicit operator T(PooledRef<T> interned) => interned.Value;

    public override string ToString() => Value?.ToString() ?? "";
}