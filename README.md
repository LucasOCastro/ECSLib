This is an archetype-based [ECS](https://en.wikipedia.org/wiki/Entity_component_system) library which stores components contiguously in memory, allowing for efficient iteration of a large number of entities.

# How to use

## ECS World
To instantiate an ECS World, use the `ECS` class. Use it to create, access, modify and destroy entities and components.
```cs
ECS world = new();
```

## Components
A component is a simple struct. Every field should be of a blittable type as components are stored within archetypes as byte arrays.
This means that reference types and strings are only allowed when wrapped with a PooledRef<T> struct, further described in [LINK].

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
Entities are represented by the record struct `Entity`, formed by an unique ID and Generation int pair. The Generation field is required because IDs are recycled after an entity is destroyed.

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
