using System.Reflection;
using System.Xml;
using ECSLib.XML.Exceptions;

namespace ECSLib.XML.Tests;

public class LoopTest
{
    [SetUp]
    public void Setup()
    {
    }

    private const string Xml = $"""
                                <?xml version="1.0" encoding="utf-8"?>

                                <Defs>
                                  <Human Parent="Merchant"></Human>
                                
                                  <Villager Parent="Human"></Villager>
                                  
                                  <Merchant Parent="Villager">
                                  </Merchant>
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
        Assert.Throws<ModelDependencyLoopException>(() => factories.RegisterAllFactories(assembly));
    }
}