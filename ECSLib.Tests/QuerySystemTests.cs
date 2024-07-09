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
            List<Entity> resultList = [];
            //Query by component A and assert the result
            _world.Query(Query.With<TestComponentA>(), entity => resultList.Add(entity));
            Assert.That(resultList, Is.EquivalentTo(new[]{entityA, entityB, entityC}));
            resultList.Clear();
            
            //Query by component B and assert the result
            _world.Query(Query.With<TestComponentB>(), entity => resultList.Add(entity));
            Assert.That(resultList, Is.EquivalentTo(new[]{entityC, entityD}));
            resultList.Clear();

            //Query by component A and B and assert the result
            _world.Query(Query.With<TestComponentA, TestComponentB>(), entity => resultList.Add(entity));
            Assert.That(resultList, Is.EquivalentTo(new[]{entityC}));
            resultList.Clear();

            //Query by component A and not component B and assert the result
            _world.Query(Query.With<TestComponentA>().WithNone<TestComponentB>(), entity => resultList.Add(entity));
            Assert.That(resultList, Is.EquivalentTo(new[]{entityA, entityB}));
            resultList.Clear();

            //Query by component B and not component A and assert the result
            _world.Query(Query.With<TestComponentB>().WithNone<TestComponentA>(), entity => resultList.Add(entity));
            Assert.That(resultList, Is.EquivalentTo(new[]{entityD}));
            resultList.Clear();
        });
    }

    private class TestSystemA : BaseSystem
    {
        public override void Process(float dt, ECS world)
        {
            world.Query(Query.With<TestComponentA>(), (Entity _, ref TestComponentA a) => a.ValueInt += 1);
        }
    }

    private class TestSystemB : BaseSystem
    {
        public override void Process(float dt, ECS world)
        {
            world.Query(Query.With<TestComponentB>(), (Entity _, ref TestComponentB b) => b.ValueDbl *= 1.5);
        }
    }
    
    private class TestSystemAB : BaseSystem
    {
        public override void Process(float dt, ECS world)
        {
            world.Query(Query.With<TestComponentA, TestComponentB>(),
                (Entity _, ref TestComponentA a, ref TestComponentB b) =>
                {
                    a.ValueInt = 0;
                    b.ValueDbl = 0;
                });
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