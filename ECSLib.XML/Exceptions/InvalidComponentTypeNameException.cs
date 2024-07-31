using System.Reflection;

namespace ECSLib.XML.Exceptions;

public class InvalidComponentTypeNameException(string componentTypeName, Assembly assembly)
    : Exception($"{componentTypeName} does not match a real component type name in assembly {assembly.FullName}.");