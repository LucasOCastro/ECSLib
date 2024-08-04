using ECSLib.Components.Interning;

namespace ECSLib.Tests.Interning;

public class InterningTests
{
    private ECS _world;

    [SetUp]
    public void Setup()
    {
        _world = new();
    }

    [Test]
    public void TestInterningUniqueness()
    {
        var a = _world.CreateEntity();
        RefPoolContext.BeginContext(a, _world);
        _world.AddComponent<CompA>(a);
        RefPoolContext.EndContext(a, _world);

        var b = _world.CreateEntity();
        RefPoolContext.BeginContext(b, _world);
        _world.AddComponent<CompB>(b);
        RefPoolContext.EndContext(b, _world);
        
        Assert.Multiple(() =>
        {
            Assert.That(_world.GetComponent<CompB>(b).Text.Value, Is.EqualTo("B"));
            Assert.That(_world.GetComponent<CompA>(a).Text.Value, Is.EqualTo("A"));
        });
        
        _world.GetComponent<CompA>(a).Text.Value += 'A';
        Assert.Multiple(() =>
        {
            Assert.That(_world.GetComponent<CompA>(a).Text.Value, Is.EqualTo("AA"));
            Assert.That(_world.GetComponent<CompB>(b).Text.Value, Is.EqualTo("B"));
        });
    }

    [Test]
    public void TestComplexInterning()
    {
        var a = _world.CreateEntity();
        RefPoolContext.BeginContext(a, _world);
        _world.AddComponent<CompComplex>(a);
        RefPoolContext.EndContext(a, _world);
        
        Assert.Multiple(() =>
        {
            ref var comp = ref _world.GetComponent<CompComplex>(a);
            Assert.That(comp.Array.Value, Is.EquivalentTo(new [] {1, 2, 3}));
            Assert.That(comp.List.Value, Is.EquivalentTo(new [] { 1.1f, 2.2f, 3.3f }));
        });
        
        ref var comp = ref _world.GetComponent<CompComplex>(a);
        for (var i = 0; i < comp.Array.Value.Length; i++)
        {
            comp.Array.Value[i]--;
        }
        Assert.That(comp.Array.Value, Is.EquivalentTo(new [] {0, 1, 2}));
        
        comp.List.Value.Insert(0, -1.5f);
        Assert.That(comp.List.Value, Is.EquivalentTo(new [] {-1.5f, 1.1f, 2.2f, 3.3f}));

        comp.List.Value = [150];
        Assert.That(comp.List.Value, Is.EquivalentTo(new [] {150}));

        comp.Array.Value = null!;
        Assert.That(comp.Array.Value, Is.Null);
        
        comp.Array.Value = new int[10];
        Assert.That(comp.Array.Value, Has.Length.EqualTo(10));
    }
}