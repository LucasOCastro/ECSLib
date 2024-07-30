namespace ECSLib.XML.Exceptions;

internal class InvalidComponentTypeNameException(string componentTypeName)
    : Exception($"{componentTypeName} does not match a real component type name.");