using ECSLib.XML.Exceptions;

namespace ECSLib.XML;

/// <summary>
/// Stores the traversed models when handling inheritance to detect loops.
/// </summary>
internal class TravelLog
{
    public HashSet<EntityModel> TraveledModels { get; } = [];
    public EntityModel Origin { get; }
    public EntityModel Current { get; private set; }

    public TravelLog(EntityModel origin)
    {
        Origin = origin;
        Current = origin;
        TraveledModels.Add(origin);
    }

    public void Step(EntityModel model)
    {
        if (!TraveledModels.Add(model))
        {
            throw new ModelDependencyLoopException(Origin.Name, Current.Name);
        }
        Current = model;
    }

    public void StepBack(EntityModel model)
    {
        TraveledModels.Remove(model);
    }
}