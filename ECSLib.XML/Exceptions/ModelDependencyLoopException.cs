namespace ECSLib.XML.Exceptions;

public class ModelDependencyLoopException(string origin, string current)
    : Exception($"{origin} depends on {current}, which also depends on {origin}.");