namespace ECSLib.Extensions;

public static class SetExtension
{
    public static bool ContainsAll<T>(this ISet<T> set, IEnumerable<T> all) => all.All(set.Contains);

    public static bool ContainsAny<T>(this ISet<T> set, IEnumerable<T> any) => any.Any(set.Contains);

    public static bool ContainsNone<T>(this ISet<T> set, IEnumerable<T> none) => !none.Any(set.Contains);
    
    /// <returns>A new <see cref="HashSet{T}"/> with only elements which appear in all provided sets.</returns>
    public static HashSet<T> Intersection<T>(this IEnumerable<ISet<T>> sets)
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
    public static HashSet<T> Union<T>(this IEnumerable<ISet<T>> sets)
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