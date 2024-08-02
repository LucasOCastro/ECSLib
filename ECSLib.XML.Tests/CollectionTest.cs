using System.Reflection;
using System.Xml;

namespace ECSLib.XML.Tests;

public class CollectionTest
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
                                 <Villager>
                                   <ECSLib.XML.Tests.DialogueComponent>
                                     <PricesList>
                                         <li>15</li>
                                         <li>200</li>
                                     </PricesList>
                                     <Array/>
                                     <Set>
                                         <li>500</li>
                                         <li>500</li>
                                         <li>6</li>
                                     </Set>
                                     <Dialogues>
                                         <li>Custom Dialogue</li>
                                     </Dialogues>
                                   </ECSLib.XML.Tests.DialogueComponent>
                                 </Villager>
                               </Defs>
                               """;

    [Test]
    public void Test1()
    {
        EntityFactoryRegistry factories = new();
        XmlDocument doc = new();
        doc.LoadXml(Xml);
        factories.LoadXml(doc);
        var assembly = Assembly.GetExecutingAssembly();
        factories.RegisterAllFactories(assembly);
        
        var villager = factories.CreateEntity("Villager", _world);
        Assert.Multiple(() =>
        {
            var dialogue = _world.GetComponent<DialogueComponent>(villager);
            Assert.That(dialogue.PricesList.Value, Is.EquivalentTo(new []{15,200}));
            Assert.That(dialogue.Array.Value, Is.Empty);
            Assert.That(dialogue.Set.Value, Is.EquivalentTo(new ulong[]{500, 6}));
            Assert.That(dialogue.Dialogues.Value, Is.EquivalentTo(new []{"Custom Dialogue"}));
        });
    }
}