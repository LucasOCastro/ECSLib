namespace ECSLib.Extensions;

public static class CollectionExtension
{
    /// <returns>A hashcode made from every element within the collection, order doesn't matter.</returns>
    public static int HashContent<T>(this IEnumerable<T> col) =>
        col.Select(t => t?.GetHashCode() ?? 0).DefaultIfEmpty().Order().Aggregate(HashCode.Combine);
    /*public static int HashContent<T>(this IEnumerable<T> col)
    {
        HashCode hash = new();
        foreach (var obj in col)
        {
            hash.Add(obj);
        }
        return hash.ToHashCode();
    }*/
}