This is an archetype-based [ECS](https://en.wikipedia.org/wiki/Entity_component_system) library which stores components contiguously in memory, allowing for efficient iteration of a large number of entities.

# How to use

## ECS World
To instantiate an ECS World, use the `ECS` class. Use it to create, access, modify and destroy entities and components.
```cs
ECS world = new();
```

The ECS object manages entities, archetypes and components internally. Systems, however, must be registed manually or via refleciton and executed by calling `ECS.ProcessSystems` (if necessary, _deltaTime_ and other such values must be stored in an external state). The execution of these systems can be customized by using [Pipelines](#pipelines).

## Components
A component is a simple struct. Every field should be of a blittable type as components are stored within archetypes as byte arrays.
This means that reference types and strings are only allowed when wrapped with the `ECSLib.Components.Interning.PooledRef<T>` struct, further described in [Reference Interning](#reference-interning).

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

Entities are stored within archetypes, and whenever a component is added or removed from an entity, it is copied to the storage of the next archetype. 
To avoid moving repeatedly moving an entity from archetype to archetype when initializing it, you can insert an entity directly into an archetype.
In this scenario, you **MUST** initialize the values in the components MANUALLY.
```cs
Entity entity = world.CreateEntityWithComponents([typeof(Comp1), typeof(Comp2)]);
world.GetComponent<Comp1>(entity) = new();
world.GetComponent<Comp2>(entity) = new();
```

## Queries
Construct a `ECSLib.Query` struct to filter and iterate through entities. 
Components passed into `With` are all required, queries with 0 required components are not supported.
Components passed into `WithAny` follow an OR pattern, so only entities with at least one of the components will be processd.
Components passed into `WithNone` are blacklisted, so entities with these components will be ignored.
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

## Systems
A system class inherits from the abstract `ECSLib.Systems.BaseSystem` class. Systems are prefereably stateless. Systems can be registered manually or via reflection ruled by attributes.
It can be processed directly by invoking the `Process` method and passing in the ECS world, or processed by the world automatically when `ECS.ProcessSystems` is called, following Pipeline rules. 
The ECS world can hold only one system of a certain type.

### Basic System
A basic system overrides the `Process` method and implements its own logic to execute. 
Annotating the system class with `ECSLib.Systems.Attributes.ECSSystemClassAttribute` is optional, 
but required if you want systems to be registered automatically via reflection when the ECS world is constructed.
```cs
public class MySystem : BaseSystem
{
  public override void Process(ECS world)
  {
    world.Query(Query.With<MyComponent>(), (Entity e, ref Comp<MyComponent> comp) => DoSomething(comp.X));
  }
}
```

### System Method
If a system class is annotated with `ECSLib.Systems.Attributes.ECSSystemClassAttribute`, its methods can be annotated with `ECSLib.Systems.Attributes.ECSSystemAttribute`
and source generation will automatically override the Process method to execute the system method, generating a Query and adapting the parameters adequately into a QueryAction.

The method can be private, public, static or instance, but should generally be static.

The first `Entity` parameter is optional.

A parameter prefixed by `ref` is read-write, a parameter prefixed by `in` is read-only.

A parameter annotated with `ECSLib.Systems.Attributes.OptionalAttribute` and wrapped with `Comp<T>` is optional.

The parameters annotated with `ECSLib.Systems.Attributes.AnyAttribute` and wrapped with `Comp<T>` will be included in the OR pattern, so at least one will be required during the query execution.

Any query information not written in the parameters (required components which won't be used, blacklisted components, etc) can be provided in the parameters of the `ECSSystemAttribute` attribute.
```cs
[ECSSystemClass]
public class MySystem : BaseSystem
{
  [ECSSystemAttribute(All=[typeof(RequiredComponentThatWontBeUsed)], None=[typeof(BlacklistedComponent)])]
  private static void MyFirstSystem(Entity entity,
                                    ref MyRequiredComp requiredComp,
                                    in MyReadOnlyComp readonlyComp,
                                    [Optional]ref Comp<MyOptionalComp> optionalComp,
                                    [Any]ref Comp<MyAnyComp1> anyComp1,
                                    [Any]ref Comp<MyAnyComp2> anyComp2)
    {
    }
}
```

### Pipelines
Define a Pipeline enum with `ECSLib.Systems.Attributes.PipelineEnumAttribute`.
Register this type by passing it to the ECS world constructor or via automatic reflection search.
Associate a system with a pipeline by providing the pipeline value in the `ECSSystemClass` attribute.
`ECS.ProcessSystems` will be executed sequentially given the numerical values of the Pipeline enum.
Systems associated with pipeline items annotated with `ECSLib.Systems.Attributes.DoNotProcessAttribute` will be ignored in `ECS.ProcessSystems`.

```cs
[PipelineEnum]
public enum MyPipeline
{
    Input = 1,
    Physics = 2,
    [DoNotProcess] Render = 3
}

[ECSSystemClass(Pipeline=(int)MyPipeline.Render)]
public class MyExampleSystem{}

private ECS _world;
public void MyGameInit(){
    _world = new(pipelineEnumType: typeof(MyPipeline));
}

public void MyGameUpdate(){
    //Processes the Input and Physics pipelines.
    _world.ProcessSystems();
}

public void MyGameRender(){
    //Processes only the render pipeline
    _world.ProcessSystems((int)MyPipeline.Render);
}
```

## Reference Interning
Components can't store references and collections, including strings. This is circumvented by interning references: Allocate the reference type in an external pool and store a local ID which maps into the pool.
This is achieved in ECSLib by using `PooledRef<T>` and `RefPool<T>`.

```cs
public struct MyCompWithRefs
{
    public PooledRef<string> MyString = new("Default values are supported!");
    public PooledRef<List<int>> MyList = new([1, 2, 3]);
}
```

**ATTENTION:** When an entity is destroyed, its PooledRefs must be released from the pool. 
To accomplish this, before registering any PooledRef, you must first set the global context in `ECSLib.Components.Interning.RefPoolContext`.
```cs
Entity e = world.CreateEntity();
RefPoolContext.BeginContext(e, world);
world.AddComponent<MyCompWithRefs>(e);
RefPoolContext.EndContext(e, world);
```

# World State Serialization
The entire ECS World state can be serialized into a binary file using the static `ECSLib.Binary.ECSSerializer` class:

```cs
using (var stream = File.Open(SaveFilePath, FileMode.Create))
{
    using (BinaryWriter writer = new(stream))
    {
        ECSSerializer.WriteWorldToBytes(ecs, writer);
    }
}
```

You can fill an ECS world from the data in the binary file:

```cs
using (var stream = File.OpenRead(SaveFilePath))
{
    using (BinaryReader reader = new(stream))
    {
        ECSSerializer.ReadWorldFromBytes(ecs, reader);
    }
}
```

Fields in binary files are serialized by field name. If a field is renamed and you want to avoid breaking old saved states, you can tag it with `LegacyNameAttribute` so the old name is also recognized.

```cs
[LegacyName("Number", "Valu", "WrongOldName")]
public float Value;
```

# XML Archetype Definitions
Entity archetypes can be defined in XML, including initial values for fields, using the ECSLib.XML package.
XML definitions are deserialized into EntityFactory delegates, which can be accessed in the `ECSLib.XML.EntityFactoryRegistry` class.
The delgates are generated using DynamicMethods.

## XML Structure
Given the components in C#:
```cs
namespace Namespace.Qualified;

public struct MyComponent
{
    public int IntField = 3;
    public PooledRef<List<int>> MyList = new([1, 2, 3]);
    public PooledRef<Dictionary<string, bool>> MyDict = new([]);
    public bool MyBoolean;
    public MyComponent() {}
}

public struct MyFlagComponent
{
}
```

An entity definition can be constructed as following. The Definition name should be **UNIQUE** amongst all other entity definitons. 
```xml
<Defs>
    <MyEntityName>
        <Namespace.Qualified.MyComponent>
            <IntField>1</IntField>
            <MyList>
                <li>4></li>
                <li>5</li>
            </MyList>
            <MyDict>
                <li>
                    <key>KeyOne</key>
                    <value>true</value>
                </li>
            </MyDict>
            <MyBoolean/>
        </Namespace.Qualified.MyComponent>
        <Namespace.Qualified.MyFlagComponent/>
    </MyEntityName>
</Defs>
```

Fields not written in the XML will have their values set to the default stablished in C#.

Collections have their items enumerated with <li> tags.

Dictionaries have <li> items, each with a <key> and <value> pair.

Boolean fields written as an empty open/close tag (`<MyBool/>`) will be parsed as true.

Components written as an empty open/close tag (`<Namespace.Qualified.MyFlagComponent/>`) will be added without changing values.

## Inheritance
Definitions can have inheritance using the `Parent` XML attribute. Multiple inheritance is allowed, each parent separated with `;;`.
The rightmost parent takes precedence. Diamond inheritance is allowed. Inheritance loops will yield exceptions.

In the example, Child will have the components CompA, CompB, CompC, CompD, and CompA will have Value=3 due to inheriting it from the rightmost parent Mother.
```XML
<Defs>
    <Father>
      <CompA>
        <Value>2</Value>
      <CompA/>
      <CompB/>
    </Father>
    <Grandma>
      <CompA>
        <Value>3</Value>
      <CompA/>
      <CompC/>
    </Grandma>
    <Child Parent="Fater;;Mother">
      <CompD/>
    </Child>
</Defs>
```

All fields from parents will be inherited, unless the XML attribute `Inherit="false"` is set in a **component**.
In that case, instead of inheriting the value from the parent definition, the value will come directly fro m the C# constructor.

A Collection or Dictionary **field** with the XML attribute `Inherit="true"` will add the values to the parent's values instead of replacing the entire collection.

You can use the XML attribute `Ignore="true"` to remove **components** that were inherited from a parent definition.

## Entity Factory Registry
To deserialize the xml into factories, load the xml files as `XmlDocument`s, load them into `EntityFactoryRegistry` using `LoadXml`, then convert all xmls into factory delegates using `RegisterAllFactories`.
```cs
XmlDocument doc = new();
doc.Load("file path");

EntityFactoryRegistry factories = new();
factories.LoadXml(doc);

var assembly = Assembly.GetExecutingAssembly();
factories.RegisterAllFactories(assembly);
```

You can get the factory itself or instantiate an entity directly from its unique name defined in XML:
```cs
Entity villager = factories.CreateEntity("Villager", world);
```

## Custom XML Parsing
Using the `ECSLib.XML.Parsing` namespace, you can register custom parsers in `StringParserManager` to to deserialize text values into structs. Implement the `IConstructorParser` interface, which parses a string into a `ParsedConstructor`. This struct representes a constructor and its parameters which will later be actually deserialized during Factory generation. 

Here's an example which will parse `(5, 10)` into `new Vector2(5, 10)`.

```cs
public class Vector2Parser : IConstructorParser
{
    public ParsedConstructor Parse(string str, Type type)
    {
        var split = str.Trim().Trim('(', ')').Split(',');
        var constructor = type.GetConstructor([typeof(float), typeof(float)]);
        return new(constructor, split);
    }
}
```

Register the custom parser before usage.

```cs
StringParserManager.AddParser(typeof(Vector2), new Vector2Parser());
```
