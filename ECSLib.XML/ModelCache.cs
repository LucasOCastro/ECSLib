
namespace ECSLib.XML;

/// <summary>
/// Stores models for factory generation.
/// </summary>
internal class ModelCache
{
    private readonly Dictionary<string, EntityModel> _models = new();

    private readonly FactoryXmlRegistry _xmlStorage;
    
    public IEnumerable<EntityModel> AllModels => _models.Values;
    
    public ModelCache(FactoryXmlRegistry xmlStorage)
    {
        _xmlStorage = xmlStorage;
    }
    
    /// <summary>
    /// Generate a model if it is not already registered, then return it.
    /// </summary>
    /// <remarks>Does not guarantee the returned model has its fields resolved.</remarks>
    public EntityModel Request(string name)
    {
        if (_models.TryGetValue(name, out var model)) return model;
        
        var def = _xmlStorage.Get(name);
        model = new(def, this);
        _models.Add(name, model);
        return model;
    }

    /// <summary>
    /// Process all dependencies of '<see cref="current"/>' to verify loops and resolve fields.
    /// </summary>
    /// <exception cref="Exceptions.ModelDependencyLoopException">
    /// Thrown if the recursion detects a looping dependency.
    /// </exception>
    private static void VerifyLoopAndResolveFieldsRecursive(EntityModel current, TravelLog log)
    {
        foreach (var parent in current.Parents)
        {
            log.Step(parent);
            VerifyLoopAndResolveFieldsRecursive(parent, log);
            log.StepBack(parent);
        }
        
        //If we can guarantee the current model is valid, it means all its dependencies were already
        //processed and had their fields resolved. Therefore, it is possible to resolve its fields here.
        if (!current.ClearedOfLoops)
        {
            current.ResolveFields();
            current.ClearedOfLoops = true;
        }
    }

    /// <summary>
    /// Processes each xml registered in <see cref="FactoryXmlRegistry"/>
    /// into a properly resolved <see cref="EntityModel"/>.
    /// </summary>
    /// <exception cref="Exceptions.ModelDependencyLoopException">
    /// Thrown if the recursion detects a looping dependency.
    /// </exception>
    public void InitializeAllAndVerifyLoops()
    {
        foreach (var name in _xmlStorage.AllNames)
        {
            var model = Request(name);
            if (!model.ClearedOfLoops) 
                VerifyLoopAndResolveFieldsRecursive(model, new(model));
        }
    }
    
    public void Clear() => _models.Clear();
}