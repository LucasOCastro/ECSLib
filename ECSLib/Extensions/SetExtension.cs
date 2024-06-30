namespace ECSLib.Extensions;

public static class SetExtension
{
    public static bool ContainsAll<T>(this IReadOnlySet<T> set, IEnumerable<T> all) => all.All(set.Contains);

    public static bool ContainsAny<T>(this IReadOnlySet<T> set, IEnumerable<T> any) => any.Any(set.Contains);

    public static bool ContainsNone<T>(this IReadOnlySet<T> set, IEnumerable<T> none) => !none.Any(set.Contains);

    public static HashSet<T> Except<T>(this IReadOnlySet<T> set, T obj)
    {
        HashSet<T> result = [..set];
        result.Remove(obj);
        return result;
    }

    public static HashSet<T> Include<T>(this IReadOnlySet<T> set, T obj) => [..set, obj];
    
    /// <returns>A new <see cref="HashSet{T}"/> with only elements which appear in all provided sets.</returns>
    public static HashSet<T> Intersection<T>(this IEnumerable<IReadOnlySet<T>> sets)
    {
        HashSet<T>? finalSet = null;
        foreach (var set in sets)
        {
            if (finalSet == null) finalSet = new(set);
            else finalSet.IntersectWith(set);
        }
        return finalSet ?? new();
    }

    /// <returns>A new <see cref="HashSet{T}"/> with the combined elements of all provided sets.</returns>
    public static HashSet<T> Union<T>(this IEnumerable<IReadOnlySet<T>> sets)
    {
        HashSet<T>? finalSet = null;
        foreach (var set in sets)
        {
            if (finalSet == null) finalSet = new(set);
            else finalSet.UnionWith(set);
        }
        return finalSet ?? new();
    }
}