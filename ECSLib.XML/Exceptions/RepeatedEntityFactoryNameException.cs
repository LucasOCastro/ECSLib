namespace ECSLib.XML.Exceptions;

public class RepeatedEntityFactoryNameException(string name) 
    : Exception($"Entity Factory was already registered for the name {name}.");