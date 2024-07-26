namespace ECSLib.Tests.Reflections;

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
        _world.AddComponent<TestComponent>(entity, new(){Value = 1});
        
        //Process the systems and assert the result
        _world.ProcessSystems();
        Assert.That(_world.GetComponent<TestComponent>(entity).Value, Is.EqualTo(2));
    }
}