﻿using System.Reflection;
using System.Xml;
using ECSLib.Entities;
using ECSLib.XML.Exceptions;

namespace ECSLib.XML;

/// <summary>
/// Stores and manages factory delegates generated from XML documents.
/// </summary>
public class EntityFactoryRegistry
{
    private readonly FactoryXmlRegistry _xmlRegistry;
    private readonly ModelCache _models;
    private readonly Dictionary<string, EntityFactoryDelegate> _factories = new();

    public EntityFactoryRegistry()
    {
        _xmlRegistry = new();
        _models = new(_xmlRegistry);
    }
    
    /// <summary>
    /// Registers a single XML document for entity generation.
    /// </summary>
    public void LoadXml(XmlDocument document) => _xmlRegistry.RegisterDocument(document);
    
    /// <summary>
    /// Registers multiple XML documents for entity generation.
    /// </summary>
    public void LoadXml(IEnumerable<XmlDocument> documents) => _xmlRegistry.RegisterDocuments(documents);

    /// <summary>
    /// After the factories are generated, call this to clean up
    /// unnecessary generation data, such as the deserialized xml documents.
    /// </summary>
    public void ClearFactoryGenerationData()
    {
        _xmlRegistry.Clear();
        _models.Clear();
    }

 
    #region FACTORY_STORAGE

    /// <summary>
    /// Generates a factory method for each XML document loaded.
    /// </summary>
    public void RegisterAllFactories(Assembly assembly)
    {
        _models.InitializeAllAndVerifyLoops();
        foreach (var model in _models.AllModels)
        {
            Register(model.Name, FactoryGenerator.CreateEntityFactory(model, assembly));
        }
    }
    
    /// <summary>
    /// Manually sets a custom factory method for a specific name.
    /// </summary>
    /// <exception cref="DuplicatedEntityFactoryNameException">
    /// Thrown if a factory was already registered for <see cref="name"/>.
    /// </exception>
    public void Register(string name, EntityFactoryDelegate factory)
    {
        if (!_factories.TryAdd(name, factory))
        {
            throw new DuplicatedEntityFactoryNameException(name);
        }
    }
    
    /// <returns>The factory delegate assigned to the provided name.</returns>
    public EntityFactoryDelegate GetFactory(string name) => _factories[name];
    
    /// <summary>
    /// Registers an entity in the ECS world using
    /// a factory delegate assigned to the provided name.
    /// </summary>
    public Entity CreateEntity(string name, ECS world) => _factories[name](world);
    
    
    #endregion
}