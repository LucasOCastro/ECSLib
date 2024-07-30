namespace ECSLib.XML.Exceptions;

internal class ModelDependencyLoopException(TravelLog traveledModels)
    : Exception($"{traveledModels.Origin} depends on {traveledModels.Current}, which also depends on {traveledModels.Origin}.");