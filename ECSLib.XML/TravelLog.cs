using ECSLib.XML.Exceptions;

namespace ECSLib.XML;

public class TravelLog
{
    public HashSet<string> TraveledModels { get; } = [];
    public string Origin { get; }
    public string Current { get; private set; }

    public void Step(string model)
    {
        if (!TraveledModels.Add(model))
        {
            throw new ModelDependencyLoopException(this);
        }
        Current = model;
    }

    public TravelLog(string origin)
    {
        Origin = origin;
        Current = origin;
        TraveledModels.Add(origin);
    }
}