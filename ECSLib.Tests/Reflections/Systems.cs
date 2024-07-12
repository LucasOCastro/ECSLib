using ECSLib.Systems;
using ECSLib.Systems.Attributes;

namespace ECSLib.Tests.Reflections;

[ECSSystemClass]
internal partial class TestSystemReflection : BaseSystem
{
    [ECSSystem]
    private static void IncrementSystem(ref TestComponent comp) => comp.Value += 1;
}