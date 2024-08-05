This is an archetype-based [ECS](https://en.wikipedia.org/wiki/Entity_component_system) library which stores components contiguously in memory, allowing for efficient iteration of a large number of entities.

# How to use

## ECS World
To instantiate an ECS World, use the `ECS` class. Use it to create, access, modify and destroy entities and components.
```cs
ECS world = new();
```

The ECS object manages entities, archetypes and components internally. Systems, however, must be registed manually (or via reflection, as explained in [LINK]) and executed by calling `ECS.ProcessSystems` (if necessary, _deltaTime_ and other such values must be stored in an external state). The execution of these systems can be customized by using Pipelines [LINK].

## Components
A component is a simple struct. Every field should be of a blittable type as components are stored within archetypes as byte arrays.
This means that reference types and strings are only allowed when wrapped with the `ECSLib.Components.Interning.PooledRef<T>` struct, further described in [LINK].

To strictly follow ECS principles, a component struct should only have public fields and a default constructor.
Because that isn't very flexible, properties which interface with fields are generally acceptable,
but methods and complex constructors should be avoided because components are not supposed to contain behavior.

```cs
struct MoverComponent
{
  public float Speed = 4.5f;
  public bool CanJump = true;
  public MoverComponent() {}
}
```

## Entities
Entities are represented by the record struct `ECSLib.Entities.Entity`, formed by an unique ID and Generation int pair. The Generation field is required because IDs are recycled after an entity is destroyed.

An entity can only have a single component of each type, trying to add duplicated components will throw an exception, as will trying to remove a component which is not present.
The `AddComponent` method receives an optional default value.

```cs
Entity entity = world.CreateEntity();
world.AddComponent<CompA>(entity);
world.RemoveComponent<CompA>(entity);
world.AddComponent<MoverComponent>(entity, new MoverComponent(){Speed = 10});
```

You can access a reference to the component with GetComponent, but trying to access a component which is not present will throw an exception. The reference can be read and written to.
```cs
ref MoverComponent comp = ref world.GetComponent<MoverComponent>(entity);
comp.Speed *= 2;
```

The entire component can also be overwritten:
```cs
world.GetComponent<MoverComponent>(entity) = new(){Speed = 0, CanJump = false};
```

## Queries
Construct a `ECSLib.Query` struct to filter and iterate through entities.
```cs
Query query = Query
              .With<RequiredComponent1, RequiredComponent2...>().
              .WithAny<OptionalComponent1, OptionalComponent2...>().
              .WithNone<UnwantedComponent1, UnwantedComponent2...>();
```

Apply the query and execute a QueryAction delegate via the ECS world object. The Comp struct is used to support optional components.
ECS.Query methods with an appropriate number of components in its action are provided dynamically via the ECSLib.SourceGen package.
```cs
world.Query(query, (Entity e, ref Comp<RequiredComponent1> c1, ref Comp<OptionalComponent1> o1) => {
    c1.Value.FieldInComponent++;
    if (o1.HasValue)
        o1.Value.FieldInOptional++;
});
```

**Attention:** Instead of using the `ECS.Query` method directly, consider using a system class as explained in [LINK].
