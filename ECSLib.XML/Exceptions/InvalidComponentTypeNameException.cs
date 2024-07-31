namespace ECSLib.XML.Exceptions;

public class InvalidComponentTypeNameException(string componentTypeName)
    : Exception($"{componentTypeName} does not match a real component type name.");