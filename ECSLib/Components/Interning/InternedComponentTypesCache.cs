namespace ECSLib.Components.Interning;

public static class InternedComponentTypesCache
{
    private static readonly Dictionary<Type, bool> Processed = [];
    
    /// <returns>true if the type has any field of type <see cref="PooledRef{T}"/>, false otherwise.</returns>
    public static bool HasInternedField(Type type)
    {
        if (!Processed.TryGetValue(type, out var result))
        {
            result = SearchForInternedField(type);
            Processed.Add(type, result);
        }
        return result;
    }

    private static bool SearchForInternedField(Type type) =>
        type.GetFields()
            .Any(f =>
                f.FieldType.IsConstructedGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(PooledRef<>)
            );
}