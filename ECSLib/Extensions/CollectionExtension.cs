namespace ECSLib.Extensions;

public static class CollectionExtension
{
    public static IEnumerable<T> Except<T>(this IEnumerable<T> col, T val) =>
        col.Where(obj => val != null && !val.Equals(obj));

    public static int HashContent<T>(this IEnumerable<T> col)
    {
        HashCode hash = new();
        foreach (var obj in col)
        {
            hash.Add(obj);
        }
        return hash.ToHashCode();
    }
}