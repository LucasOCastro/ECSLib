using System.Xml;
using ECSLib.Entities;
using ECSLib.XML.Exceptions;

namespace ECSLib.XML;

public static class EntityFactoryRegistry
{
    private static readonly Dictionary<string, EntityFactoryDelegate> Factories = new();
    
    public static void Load(string name, EntityFactoryDelegate factory)
    {
        if (!Factories.TryAdd(name, factory))
        {
            throw new RepeatedEntityFactoryNameException(name);
        }
    }

    public static void LoadFrom(XmlDocument document) =>
        Load(document.Name, EntityDeserializer.CreateEntityFactory(document));

    public static void LoadAllInDirectory(string dir, bool includeSubdirectories)
    {
        foreach (var file in Directory.EnumerateFiles(dir, "*.xml"))
        {
            XmlDocument doc = new();
            doc.Load(file);
            LoadFrom(doc);
        }

        if (includeSubdirectories)
            foreach (var sub in Directory.EnumerateDirectories(dir))
                LoadAllInDirectory(sub, includeSubdirectories);
    }

    public static EntityFactoryDelegate GetFactory(string name) => Factories[name];
    
    public static Entity CreateNew(string name, ECS world) => Factories[name](world);

    public static void Clear() => Factories.Clear();

    public static void Remove(string name) => Factories.Remove(name);
}