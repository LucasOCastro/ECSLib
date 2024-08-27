using ECSLib.Components.Interning;

namespace ECSLib.Tests.Saving;

public struct CompA
{
    public int Value = 10;
    public PooledRef<List<OneClass>> Classes = new([]);
    
    public CompA(){}
}

public struct CompB
{
    public PooledRef<string> Text = new("Yo");
    
    public CompB(){}
}

public class OneClass
{
    public int Prop { get; set; }
}