using ECSLib.Systems;
using ECSLib.Systems.Attributes;

namespace ECSLib.Tests.QuerySystem;

[ECSSystemClass]
internal partial class TestSystemA : BaseSystem
{
    [ECSSystem]
    private void IncrementA(ref TestComponentA a)
    {
        a.ValueInt += 1;
    }
}

[ECSSystemClass]
internal partial class TestSystemB : BaseSystem
{
    [ECSSystem]
    private void MultiplyB(ref TestComponentB b)
    {
        b.ValueDbl *= 1.5;
    }
}
    
[ECSSystemClass]
// ReSharper disable once InconsistentNaming
internal partial class TestSystemAB : BaseSystem
{
    [ECSSystem]
    private void ZeroBoth(ref TestComponentA a, ref TestComponentB b)
    {
        a.ValueInt = 0;
        b.ValueDbl = 0;
    }
}