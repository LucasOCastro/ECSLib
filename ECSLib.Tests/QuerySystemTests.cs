using ECSLib.Entities;
using ECSLib.Systems;
using ECSLib.Systems.Exceptions;

namespace ECSLib.Tests;

[Order(2)]
public class QuerySystemTests
{
    private struct TestComponentA
    {
        public int ValueInt;
    }

    private struct TestComponentB
    {
        public double ValueDbl;
    }
    
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
    
    [Test, Order(3)]
    public void TestQueries()
    {
        //Create two entities with TestComponentA
        var entityA = _world.CreateEntity();
        _world.AddComponent<TestComponentA>(entityA, new(){ValueInt = 0});
        var entityB = _world.CreateEntity();
        _world.AddComponent<TestComponentA>(entityB, new(){ValueInt = 1});
        
        //Create one entity with TestComponentA and TestComponentB
        var entityC = _world.CreateEntity();
        _world.AddComponent<TestComponentA>(entityC, new(){ValueInt = 2});
        _world.AddComponent<TestComponentB>(entityC, new(){ValueDbl = 2.1});
        
        //Create one entity with TestComponentB
        var entityD = _world.CreateEntity();
        _world.AddComponent<TestComponentB>(entityD, new(){ValueDbl = 3.0});
        
        //Create one entity with no component
        var entityE = _world.CreateEntity();
        Assert.Multiple(() =>
        {
            //Query by component A and assert the result
            Assert.That(_world.Query(Query.All(typeof(TestComponentA))).ToHashSet().SetEquals([entityA, entityB, entityC]));

            //Query by component B and assert the result
            Assert.That(_world.Query(Query.All(typeof(TestComponentB))).ToHashSet().SetEquals([entityC, entityD]));

            //Query by component A and B and assert the result
            Assert.That(_world.Query(Query.All(typeof(TestComponentB), typeof(TestComponentA))).ToHashSet().SetEquals([entityC]));

            //Query by component A or component B and assert the result
            Assert.That(_world.Query(Query.Any(typeof(TestComponentA), typeof(TestComponentB))).ToHashSet().SetEquals([entityA, entityB, entityC, entityD]));

            //Query by component A and not component B and assert the result
            Assert.That(_world.Query(Query.All(typeof(TestComponentA)).WithNone(typeof(TestComponentB))).ToHashSet().SetEquals([entityA, entityB]));

            //Query by component B and not component A and assert the result
            Assert.That(_world.Query(Query.All(typeof(TestComponentB)).WithNone(typeof(TestComponentA))).ToHashSet().SetEquals([entityD]));
        });
    }

    private class TestSystemA : BaseSystem
    {
        protected override Query GetQuery() => Query.All(typeof(TestComponentA));

        protected override void Process(float dt, ECS world, IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                world.GetComponent<TestComponentA>(entity).ValueInt += 1;
            }
        }
    }

    private class TestSystemB : BaseSystem
    {
        protected override Query GetQuery() => Query.All(typeof(TestComponentB));

        protected override void Process(float dt, ECS world, IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                world.GetComponent<TestComponentB>(entity).ValueDbl *= 1.5;
            }
        }
    }
    
    private class TestSystemAB : BaseSystem
    {
        protected override Query GetQuery() => Query.All(typeof(TestComponentA), typeof(TestComponentB));

        protected override void Process(float dt, ECS world, IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                world.GetComponent<TestComponentA>(entity).ValueInt = 0;
                world.GetComponent<TestComponentB>(entity).ValueDbl = 0;
            }
        }
    }
    
    [Test, Order(4)]
    public void TestSystems()
    {
        //Create one entity with TestComponentA
        var entityA = _world.CreateEntity();
        _world.AddComponent<TestComponentA>(entityA, new(){ValueInt = 1});
        
        //Create one entity with TestComponentB
        var entityB = _world.CreateEntity();
        _world.AddComponent<TestComponentB>(entityB, new(){ValueDbl = 2.0});
        
        //Create one entity with TestComponentA and TestComponentB
        var entityC = _world.CreateEntity();
        _world.AddComponent<TestComponentA>(entityC, new(){ValueInt = 3});
        _world.AddComponent<TestComponentB>(entityC, new(){ValueDbl = 3.1});
        
        //Create one entity with no component
        var entityE = _world.CreateEntity();
        
        //Register one system and assert it was registered
        _world.RegisterSystem<TestSystemA>();
        Assert.Throws<RepeatedSystemException>(() => _world.RegisterSystem<TestSystemA>());
        
        //Register other systems
        _world.RegisterSystem<TestSystemB>();
        _world.RegisterSystem<TestSystemAB>();
        
        //Run the systems and assert the new values
        _world.ProcessSystems(0);
        Assert.Multiple(() =>
        {
            Assert.That(_world.GetComponent<TestComponentA>(entityA).ValueInt, Is.EqualTo(2));
            Assert.That(_world.GetComponent<TestComponentB>(entityB).ValueDbl, Is.EqualTo(3));
            Assert.That(_world.GetComponent<TestComponentA>(entityC).ValueInt, Is.Zero);
            Assert.That(_world.GetComponent<TestComponentB>(entityC).ValueDbl, Is.Zero);
        });
    }
}