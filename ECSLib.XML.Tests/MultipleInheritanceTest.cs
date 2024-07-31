using System.Reflection;
using System.Xml;

namespace ECSLib.XML.Tests;

public class MultipleInheritanceTest
{
    private ECS _world;
    
    [SetUp]
    public void Setup()
    {
        _world = new();
    }

    private const string Xml = $"""
                                <?xml version="1.0" encoding="utf-8"?>

                                <Defs>
                                  <Merchant>
                                    <ECSLib.XML.Tests.HealthComponent>
                                        <Health>10</Health>
                                        <DeathSound>1</DeathSound>
                                    </ECSLib.XML.Tests.HealthComponent>
                                  </Merchant>
                                  
                                  <Goblin>
                                    <ECSLib.XML.Tests.HealthComponent>
                                        <Health>4</Health>
                                        <DeathSpeech>Arghh goblin!</DeathSpeech>
                                    </ECSLib.XML.Tests.HealthComponent>
                                    <ECSLib.XML.Tests.MoverComponent>
                                        <Speed>20</Speed>
                                    </ECSLib.XML.Tests.MoverComponent>
                                  </Goblin>
                                  
                                  <MerchantGoblin Parent="Merchant;;Goblin">
                                    <ECSLib.XML.Tests.HealthComponent>
                                        <DeathSpeech>Money goob</DeathSpeech>
                                    </ECSLib.XML.Tests.HealthComponent>
                                  </MerchantGoblin>
                                </Defs>
                                """;

    [Test]
    public void MultipleInheritance()
    {
        EntityFactoryRegistry factories = new();
        XmlDocument doc = new();
        doc.LoadXml(Xml);
        factories.LoadXml(doc);
        var assembly = Assembly.GetExecutingAssembly();
        Assert.DoesNotThrow(() => factories.RegisterAllFactories(assembly));

        var entity = factories.CreateEntity("MerchantGoblin", _world);
        Assert.Multiple(() =>
        {
            ref var h = ref _world.GetComponent<HealthComponent>(entity);
            ref var m = ref _world.GetComponent<MoverComponent>(entity);
            Assert.That(h.Health, Is.EqualTo(4));
            Assert.That(h.DeathSound, Is.EqualTo(1));
            Assert.That(h.DeathSpeech.Value, Is.EqualTo("Money goob"));
            Assert.That(m.Speed, Is.EqualTo(20));
        });
    }
    
    private const string DiamondXml = $"""
                                <?xml version="1.0" encoding="utf-8"?>
                                
                                <Defs>
                                    <MerchantGoblin Parent="Merchant;;Goblin">
                                      <ECSLib.XML.Tests.HealthComponent>
                                          <DeathSpeech>Money goob</DeathSpeech>
                                      </ECSLib.XML.Tests.HealthComponent>
                                    </MerchantGoblin>
                                    
                                    <Creature>
                                        <ECSLib.XML.Tests.HealthComponent>
                                            <Health>1</Health>
                                        </ECSLib.XML.Tests.HealthComponent>
                                        <ECSLib.XML.Tests.MoverComponent>
                                            <Speed>2</Speed>
                                            <CanRun/>
                                        </ECSLib.XML.Tests.MoverComponent>
                                    </Creature>
                                
                                  <Merchant Parent="Creature">
                                    <ECSLib.XML.Tests.HealthComponent>
                                        <Health>10</Health>
                                        <DeathSound>1</DeathSound>
                                    </ECSLib.XML.Tests.HealthComponent>
                                  </Merchant>
                                  
                                  <Goblin Parent="Creature">
                                    <ECSLib.XML.Tests.HealthComponent>
                                        <Health>4</Health>
                                        <DeathSpeech>Arghh goblin!</DeathSpeech>
                                    </ECSLib.XML.Tests.HealthComponent>
                                    <ECSLib.XML.Tests.MoverComponent>
                                        <Speed>20</Speed>
                                    </ECSLib.XML.Tests.MoverComponent>
                                  </Goblin>
                                  
                                </Defs>
                                """;
    
    [Test]
    public void DiamondInheritance()
    {
        EntityFactoryRegistry factories = new();
        XmlDocument doc = new();
        doc.LoadXml(DiamondXml);
        factories.LoadXml(doc);
        var assembly = Assembly.GetExecutingAssembly();
        Assert.DoesNotThrow(() => factories.RegisterAllFactories(assembly));

        var entity = factories.CreateEntity("MerchantGoblin", _world);
        Assert.Multiple(() =>
        {
            ref var h = ref _world.GetComponent<HealthComponent>(entity);
            ref var m = ref _world.GetComponent<MoverComponent>(entity);
            Assert.That(h.Health, Is.EqualTo(4));
            Assert.That(h.DeathSound, Is.EqualTo(1));
            Assert.That(h.DeathSpeech.Value, Is.EqualTo("Money goob"));
            Assert.That(m.Speed, Is.EqualTo(20));
            Assert.That(m.CanRun, Is.True);
        });
    }
}