using System.Reflection;
using System.Xml;
using ECSLib.Components.Exceptions;

namespace ECSLib.XML.Tests;

public class InheritanceAttributeTest
{
     private ECS _world;
    
    [SetUp]
    public void Setup()
    {
        _world = new();
    }

    private const string Xml = """
                                <?xml version="1.0" encoding="utf-8"?>

                                <Defs>
                                  <Base>
                                    <ECSLib.XML.Tests.HealthComponent>
                                        <Health>10</Health>
                                        <DeathSound>1</DeathSound>
                                    </ECSLib.XML.Tests.HealthComponent>
                                    <ECSLib.XML.Tests.MoverComponent>
                                        <Speed>20</Speed>
                                    </ECSLib.XML.Tests.MoverComponent>
                                    <ECSLib.XML.Tests.DialogueComponent>
                                        <PricesList>
                                            <li>15</li>
                                            <li>200</li>
                                        </PricesList>
                                        <Set>
                                            <li>500</li>
                                            <li>500</li>
                                            <li>6</li>
                                        </Set>
                                    </ECSLib.XML.Tests.DialogueComponent>
                                  </Base>
                                  
                                  <Child Parent="Base">
                                    <ECSLib.XML.Tests.HealthComponent Inherit="false">
                                        <DeathSpeech>Arghh goblin!</DeathSpeech>
                                    </ECSLib.XML.Tests.HealthComponent>
                                    <ECSLib.XML.Tests.MoverComponent Ignore="true"/>
                                    <ECSLib.XML.Tests.DialogueComponent>
                                        <PricesList Inherit="true">
                                            <li>1000</li>
                                        </PricesList>
                                        <Set Inherit="false">
                                            <li>1000</li>
                                        </Set>
                                    </ECSLib.XML.Tests.DialogueComponent>
                                  </Child>
                                </Defs>
                                """;

    [Test]
    public void InheritanceAttributes()
    {
        EntityFactoryRegistry factories = new();
        XmlDocument doc = new();
        doc.LoadXml(Xml);
        factories.LoadXml(doc);
        var assembly = Assembly.GetExecutingAssembly();
        Assert.DoesNotThrow(() => factories.RegisterAllFactories(assembly));
        
        var baseEntity = factories.CreateEntity("Base", _world);
        var childEntity = factories.CreateEntity("Child", _world);
        Assert.Multiple(() =>
        {
            ref var hBase = ref _world.GetComponent<HealthComponent>(baseEntity);
            ref var hChild = ref _world.GetComponent<HealthComponent>(childEntity);
            Assert.That(hBase.Health, Is.EqualTo(10));
            Assert.That(hChild.Health, Is.EqualTo(5));
            Assert.That(hBase.DeathSound, Is.EqualTo(1));
            Assert.That(hChild.DeathSound, Is.EqualTo(-1));
            Assert.That(hBase.DeathSpeech.Value, Is.EqualTo("Silent"));
            Assert.That(hChild.DeathSpeech.Value, Is.EqualTo("Arghh goblin!"));
            
            Assert.DoesNotThrow(() => _world.GetComponent<MoverComponent>(baseEntity));
            Assert.Throws<MissingComponentException>(() => _world.GetComponent<MoverComponent>(childEntity));

            ref var dBase = ref _world.GetComponent<DialogueComponent>(baseEntity);
            ref var dChild = ref _world.GetComponent<DialogueComponent>(childEntity);
            Assert.That(dBase.PricesList.Value, Is.EquivalentTo(new []{15, 200}));
            Assert.That(dChild.PricesList.Value, Is.EquivalentTo(new []{15, 200, 1000}));
            Assert.That(dBase.Set.Value, Is.EquivalentTo(new []{500, 6}));
            Assert.That(dChild.Set.Value, Is.EquivalentTo(new []{1000}));
        });
    }
}