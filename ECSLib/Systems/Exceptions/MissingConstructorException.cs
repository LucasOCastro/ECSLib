namespace ECSLib.Systems.Exceptions;

public class MissingConstructorException(Type type, MissingMethodException innerException) 
    : Exception($"System Type {type.Name} does not have an accessible default constructor.", innerException);