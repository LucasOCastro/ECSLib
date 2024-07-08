using ECSLib.Entities;

namespace ECSLib.Tests;

[Order(100)]
public class StressTest
{
    private struct TestComponentA
    {
        public double ValueDbl;
    }

    private struct TestComponentB
    {
        public int ValueInt;
    }
    
    private const int ACompCount = 10000;
    private const int BCompCount = 15280;
    private const int ABCompCount = 12347;
    private const int NoCompCount = 5422;
    private const int Iterations = 100;
    private const double ABaseValue = 9017.463;
    private const int BBaseValue = 40563;
    
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
    
    [Test, Order(0)]
    public void Scenario1()
    {
        for (int i = 0; i < ACompCount; i++)
        {
            var entity = _world.CreateEntity();
            _world.AddComponent<TestComponentA>(entity, new(){ValueDbl = ABaseValue});
        }
                
        for (int i = 0; i < BCompCount; i++)
        {
            var entity = _world.CreateEntity();
            _world.AddComponent<TestComponentB>(entity, new(){ValueInt = BBaseValue});
        }
        
        for (int i = 0; i < ABCompCount; i++)
        {
            var entity = _world.CreateEntity();
            _world.AddComponent<TestComponentA>(entity, new(){ValueDbl = ABaseValue});
            _world.AddComponent<TestComponentB>(entity, new(){ValueInt = BBaseValue});
        }
        
        for (int i = 0; i < NoCompCount; i++)
        {
            _world.CreateEntity();
        }
        
        //Assert all components have the right values before systems process them
        _world.Query(Query.With<TestComponentA>(),
            (Entity _, ref TestComponentA a) => Assert.That(a.ValueDbl, Is.EqualTo(ABaseValue)));
        _world.Query(Query.With<TestComponentB>(),
            (Entity _, ref TestComponentB b) => Assert.That(b.ValueInt, Is.EqualTo(BBaseValue)));
        
        _world.Query(Query.With<TestComponentA>(), (Entity _, ref TestComponentA a) =>
        {
            for (int i = 0; i < Iterations; i++)
            {
                a.ValueDbl = Math.Sqrt(a.ValueDbl);
            }
        });
        
        _world.Query(Query.With<TestComponentB>(), (Entity _, ref TestComponentB b) =>
        {
            for (int i = 0; i < Iterations; i++)
            {
                b.ValueInt = b.ValueInt / ((int)Math.Sqrt(b.ValueInt) + 1) + 500;
            }
        });
        
        _world.Query(Query.With<TestComponentA, TestComponentB>(), (Entity _, ref TestComponentA a, ref TestComponentB b) =>
        {
            for (int i = 0; i < Iterations; i++)
            {
                b.ValueInt = b.ValueInt / ((int)Math.Sqrt(a.ValueDbl) + 1) + 500;
                a.ValueDbl = b.ValueInt * 94.7f * Math.Sqrt(b.ValueInt);
            }
        });
    }
}