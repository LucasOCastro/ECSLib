namespace ECSLib.XML.Exceptions;

public class DuplicatedEntityDocumentNameException(string name)
    : Exception($"Document with name {name} was already loaded.");