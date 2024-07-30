namespace ECSLib.Components.Interning;

public struct PooledRef<T> where T: class
{
    private int _id = -1;

    public PooledRef()
    {
    }

    public PooledRef(T value)
    {
        Value = value;
    }

    public int ID
    {
        get
        {
            if (_id == -1) _id = RefPool<T>.Register();
            return _id;
        }
    }

    public T Value
    {
        get => RefPool<T>.Get(ID);
        set => RefPool<T>.Set(ID, value);
    }

    public static implicit operator T(PooledRef<T> interned) => interned.Value;

    public override string ToString() => Value.ToString();
}