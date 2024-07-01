namespace ECSLib.Tests;

[Order(2)]
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
    
    private const int ACompCount = 4500;
    private const int BCompCount = 4500;
    private const int ABCompCount = 900;
    private const int NoCompCount = 100;
    private const int Iterations = 100;
    private const double ABaseValue = 9678.463;
    private const int BBaseValue = 9970;
    
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
        Assert.DoesNotThrow(() =>
        {
            var before = DateTime.Now;
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
                    
                    
            foreach (var entity in _world.Query(Query.All(typeof(TestComponentA))))
            {
                for (int i = 0; i < Iterations; i++)
                {
                    ref var a = ref _world.GetComponent<TestComponentA>(entity);
                    a.ValueDbl = Math.Sqrt(a.ValueDbl);
                }
            }
                    
            foreach (var entity in _world.Query(Query.All(typeof(TestComponentB))))
            {
                for (int i = 0; i < Iterations; i++)
                {
                    ref var b = ref _world.GetComponent<TestComponentB>(entity);
                    b.ValueInt = b.ValueInt / ((int)Math.Sqrt(b.ValueInt) + 1) + 500;
                }
            }
                    
            foreach (var entity in _world.Query(Query.All(typeof(TestComponentA), typeof(TestComponentB))))
            {
                for (int i = 0; i < Iterations; i++)
                {
                    ref var a = ref _world.GetComponent<TestComponentA>(entity);
                    ref var b = ref _world.GetComponent<TestComponentB>(entity);
                    b.ValueInt = b.ValueInt / ((int)Math.Sqrt(a.ValueDbl) + 1) + 500;
                    a.ValueDbl = b.ValueInt * 94.7f * Math.Sqrt(b.ValueInt);
                }
            }
            var after = DateTime.Now;
            Console.Out.Write("Diff = " + (after - before).Milliseconds + "ms");
        });
    }
}