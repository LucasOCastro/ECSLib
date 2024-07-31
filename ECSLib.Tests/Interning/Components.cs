using ECSLib.Components.Interning;

namespace ECSLib.Tests.Interning;

public struct CompA
{
    public PooledRef<string> Text = new("A");

    public CompA()
    {
    }
}

public struct CompB
{
    public PooledRef<string> Text = new("B");

    public CompB()
    {
    }
}

public struct CompComplex
{
    public PooledRef<int[]> Array = new([1,2,3]);
    public PooledRef<List<float>> List = new([1.1f, 2.2f, 3.3f]);

    public CompComplex()
    {
    }
}