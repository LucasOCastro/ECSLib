using System.Reflection;
using ECSLib.Archetypes;
using ECSLib.Components.Exceptions;

namespace ECSLib.Tests;

[Order(1)]
public class ECSTests
{
    private struct TestComponentA
    {
        public int Data;
    }

    private struct TestComponentB
    {
        public bool BoolData;
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
    
    [Test, Order(0)]
    public void TestEntityCreateDestroy()
    {
        //Creates an entity and asserts it's the first entity.
        var entityA = _world.CreateEntity();
        Assert.That(entityA.ID, Is.Zero);
        
        //Creates another entity and asserts it's the next.
        //Creates an entity and asserts it's the first entity.
        var entityB = _world.CreateEntity();
        Assert.That(entityB.ID, Is.EqualTo(1));
        
        //Destroys the first entity and assert a new one will have index 0.
        _world.DestroyEntity(entityA);
        var entityC = _world.CreateEntity();
        Assert.That(entityC.ID, Is.Zero);
        
        //Assert a new entity will have index 2
        var entityD = _world.CreateEntity();
        Assert.That(entityD.ID, Is.EqualTo(2));
    }
    
    [Test, Order(1)]
    public void TestEntityComponentAddRemove()
    {
        const int testValueSet = 15;
        const int testValueInit = 20;
        
        var entity = _world.CreateEntity();
        
        //Adds a component and assert it is the default value.
        _world.AddComponent<TestComponentA>(entity);
        Assert.That(_world.GetComponent<TestComponentA>(entity), Is.EqualTo(default(TestComponentA)));
        
        //Asserts it's not possible to add a component of the same type.
        Assert.Throws<DuplicatedComponentException>(() => _world.AddComponent<TestComponentA>(entity));
        
        //Change a component's data and assert the value has changed; 
        ref var refComp = ref _world.GetComponent<TestComponentA>(entity);
        refComp.Data = testValueSet;
        Assert.That(_world.GetComponent<TestComponentA>(entity).Data, Is.EqualTo(testValueSet));
        
        //Remove a component and assert it doesn't exist anymore.
        _world.RemoveComponent<TestComponentA>(entity);
        Assert.Throws<MissingComponentException>(() => _world.GetComponent<TestComponentA>(entity));
        
        //Add a component with a predefined value and assert it is the initiated value.
        _world.AddComponent(entity, new TestComponentA{Data = testValueInit});
        Assert.That(_world.GetComponent<TestComponentA>(entity).Data, Is.EqualTo(testValueInit));
    }

    [Test, Order(2)]
    public void TestEntityArchetypeUpdate()
    {
        var managerField = typeof(ECS)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .First(f => f.FieldType == typeof(ArchetypeManager));
        var archetypeManager = managerField.GetValue(_world) as ArchetypeManager;
        Assert.That(archetypeManager, Is.Not.Null);

        var storageField = typeof(ArchetypeManager)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .First(f => f.FieldType == typeof(ArchetypeStorage));
        var archetypeStorage = storageField.GetValue(archetypeManager) as ArchetypeStorage;
        Assert.That(archetypeStorage, Is.Not.Null);
     
        //Creates an entity and asserts its archetype is empty
        var entityA = _world.CreateEntity();
        Assert.That(!archetypeManager.GetAllComponentTypes(entityA).Any());
        
        //Adds a component and assert its archetype is the component
        _world.AddComponent<TestComponentA>(entityA);
        Assert.That(archetypeManager.GetAllComponentTypes(entityA).ToHashSet().SetEquals([typeof(TestComponentA)]));
        
        //Adds another component and assert its archetype is both components
        _world.AddComponent<TestComponentB>(entityA);
        Assert.That(archetypeManager.GetAllComponentTypes(entityA).ToHashSet().SetEquals([typeof(TestComponentA), typeof(TestComponentB)]));
        
        //Creates another entity and assert their archetypes are different
        var entityB = _world.CreateEntity();
        Assert.That(archetypeStorage.GetOrCreateArchetype(archetypeManager.GetAllComponentTypes(entityA)),
            Is.Not.EqualTo(archetypeStorage.GetOrCreateArchetype(archetypeManager.GetAllComponentTypes(entityB))));
        
        //Adds the components to the entity and assert their archetypes are equal (ordered insertion)
        _world.AddComponent<TestComponentA>(entityB);
        _world.AddComponent<TestComponentB>(entityB);
        Assert.That(archetypeStorage.GetOrCreateArchetype(archetypeManager.GetAllComponentTypes(entityA)),
            Is.EqualTo(archetypeStorage.GetOrCreateArchetype(archetypeManager.GetAllComponentTypes(entityB))));
        
        //Adds the components to the entity and assert their archetypes are equal (unordered insertion)
        _world.RemoveComponent<TestComponentA>(entityB);
        _world.AddComponent<TestComponentA>(entityB);
        Assert.That(archetypeStorage.GetOrCreateArchetype(archetypeManager.GetAllComponentTypes(entityA)),
            Is.EqualTo(archetypeStorage.GetOrCreateArchetype(archetypeManager.GetAllComponentTypes(entityB))));
    }

    [Test, Order(3)]
    public void TestQueries()
    {
        //Create two entities with TestComponentA
        var entityA = _world.CreateEntity();
        _world.AddComponent<TestComponentA>(entityA, new(){Data = 0});
        var entityB = _world.CreateEntity();
        _world.AddComponent<TestComponentA>(entityB, new(){Data = 1});
        
        //Create one entity with TestComponentA and TestComponentB
        var entityC = _world.CreateEntity();
        _world.AddComponent<TestComponentA>(entityC, new(){Data = 2});
        _world.AddComponent<TestComponentB>(entityC, new(){BoolData = true});
        
        //Create one entity with TestComponentB
        var entityD = _world.CreateEntity();
        _world.AddComponent<TestComponentB>(entityD, new(){BoolData = false});
        
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
}