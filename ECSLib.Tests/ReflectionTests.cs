using ECSLib.Components;
using ECSLib.Entities;
using ECSLib.Systems;

namespace ECSLib.Tests;

public class TestSystemReflection : BaseSystem
{
    public struct TestComponent
    {
        public int Value;
    }

    public override void Process(float dt, ECS world)
    {
        world.Query(Query.With<TestComponent>(), (Entity entity, ref Comp<TestComponent> comp) => comp.Value.Value += 1);
    }
}

[Order(3)]
public class ReflectionTests
{
    private ECS _world;
    
    [SetUp]
    public void Setup()
    {
        _world = new();
    }

    [TearDown]
    public void TearDown()
    {
    }

    [Test]
    public void TestReflectionInit()
    {
        //Create a world with reflection registering
        _world = new(registerSystemsViaReflection: true);
        
        //Create an entity with the test component
        var entity = _world.CreateEntity();
        _world.AddComponent<TestSystemReflection.TestComponent>(entity, new(){Value = 1});
        
        //Process the systems and assert the result
        _world.ProcessSystems(0);
        Assert.That(_world.GetComponent<TestSystemReflection.TestComponent>(entity).Value, Is.EqualTo(2));
    }
}