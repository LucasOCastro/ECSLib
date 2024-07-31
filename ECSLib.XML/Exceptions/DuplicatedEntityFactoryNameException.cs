namespace ECSLib.XML.Exceptions;

public class DuplicatedEntityFactoryNameException(string name) 
    : Exception($"Entity Factory was already registered for the name {name}.");