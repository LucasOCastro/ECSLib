using System.Xml;
using ECSLib.Components;
using ECSLib.Entities;
using ECSLib.XML.Exceptions;

namespace ECSLib.XML.Tests;

public struct HealthComponent
{
    public int Health = 5;
    public int DeathSound = -1;

    public HealthComponent()
    {
    }
}

public struct MoverComponent
{
    public float Speed = 1.0f;
    public bool CanRun = false;

    public MoverComponent()
    {
    }
}

public class Tests
{
    private ECS _world;
    
    [SetUp]
    public void Setup()
    {
        _world = new();
    }

    [Test]
    public void Test1()
    {
        EntityFactoryRegistry factories = new();
        XmlDocument doc = new();
        doc.Load(@"D:\MonoGameProjs\ECSLib\ECSLib.XML.Tests\Villager.xml");
        factories.LoadXml(doc);
        factories.RegisterAllFactories();
        
        //Assert the entity was created properly
        var villager = factories.CreateEntity("Villager", _world);
        int i = 0;
        _world.Query(Query.With<HealthComponent, MoverComponent>(),
            (Entity e, ref Comp<HealthComponent> h, ref Comp<MoverComponent> m) =>
            {
                i++;
                Assert.That(villager.ID, Is.EqualTo(e.ID));
                Assert.That(h.Value.Health, Is.EqualTo(10));
                Assert.That(h.Value.DeathSound, Is.EqualTo(15));
                Assert.That(m.Value.Speed, Is.EqualTo(4.5f));
                Assert.That(m.Value.CanRun, Is.True);
            });
        Assert.That(i, Is.EqualTo(1));
        
        //Assert can't register multiple xml with same name
        Assert.Throws<DuplicatedEntityDocumentNameException>(() => factories.LoadXml(doc));
        
        //Assert can't register multiple factories with same name
        factories.ClearFactoryGenerationData();
        Assert.DoesNotThrow(() => factories.LoadXml(doc));
        Assert.Throws<DuplicatedEntityFactoryNameException>(() => factories.RegisterAllFactories());
    }
}