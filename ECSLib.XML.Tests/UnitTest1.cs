using System.Globalization;
using System.Reflection;
using System.Xml;
using ECSLib.Components;
using ECSLib.Entities;
using ECSLib.XML.Exceptions;

namespace ECSLib.XML.Tests;

public class Tests
{
    private ECS _world;
    
    [SetUp]
    public void Setup()
    {
        _world = new();
    }

    private const int TestHealth = 10;
    private const int TestDeathSound = 15;
    private const float TestSpeed = 4.5f;
    private const bool TestCanRun = true;
    private const float TestSpeedMerchant = 2.0f;
    private const int TestNullableModeMerchant = 14;

    // ReSharper disable once HeuristicUnreachableCode
    private static readonly string Xml = $"""
                                          <?xml version="1.0" encoding="utf-8"?>

                                          <Defs>
                                            <Villager>
                                              <ECSLib.XML.Tests.HealthComponent>
                                                  <Health>{TestHealth}</Health>
                                                  <DeathSound>{TestDeathSound}</DeathSound>
                                              </ECSLib.XML.Tests.HealthComponent>
                                              <ECSLib.XML.Tests.MoverComponent>
                                                  <Speed>{TestSpeed.ToString(CultureInfo.InvariantCulture)}</Speed>
                                                  {(TestCanRun ? "<CanRun/>" : "")}
                                              </ECSLib.XML.Tests.MoverComponent>
                                            </Villager>
                                            <Merchant Parent="Villager">
                                                <ECSLib.XML.Tests.HealthComponent>
                                                    <DeathSpeech>Argh! My farms!</DeathSpeech>
                                                    <NullableMode>{TestNullableModeMerchant}</NullableMode>
                                                </ECSLib.XML.Tests.HealthComponent>
                                                <ECSLib.XML.Tests.MoverComponent>
                                                  <Speed>{TestSpeedMerchant.ToString(CultureInfo.InvariantCulture)}</Speed>
                                                </ECSLib.XML.Tests.MoverComponent>
                                            </Merchant>
                                            <WithTag>
                                                <ECSLib.XML.Tests.TagComponent/>
                                            </WithTag>
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
        
        //Assert the entity was created properly
        var villager = factories.CreateEntity("Villager", _world);
        Assert.That(_world.GetComponent<HealthComponent>(villager).NullableMode, Is.Null);
        var merchant = factories.CreateEntity("Merchant", _world);
        int i = 0;
        _world.Query(Query.With<HealthComponent, MoverComponent>(),
            (Entity e, ref Comp<HealthComponent> h, ref Comp<MoverComponent> m) =>
            {
                i++;
                Assert.That(h.Value.Health, Is.EqualTo(TestHealth));
                Assert.That(h.Value.DeathSound, Is.EqualTo(TestDeathSound));
                Assert.That(m.Value.Speed, Is.EqualTo(e == merchant ? TestSpeedMerchant : TestSpeed));
                Assert.That(m.Value.CanRun, Is.EqualTo(TestCanRun));
                Assert.That(h.Value.NullableMode, Is.EqualTo(e == merchant ? TestNullableModeMerchant : null));
            });
        Assert.That(i, Is.EqualTo(2));

        var withTag = factories.CreateEntity("WithTag", _world);
        Assert.DoesNotThrow(() => _world.GetComponent<TagComponent>(withTag));
        
        //Assert can't register multiple xml with same name
        Assert.Throws<DuplicatedEntityDocumentNameException>(() => factories.LoadXml(doc));
        
        //Assert can't register multiple factories with same name
        factories.ClearFactoryGenerationData();
        Assert.DoesNotThrow(() => factories.LoadXml(doc));
        Assert.Throws<DuplicatedEntityFactoryNameException>(() => factories.RegisterAllFactories(assembly));
    }
}