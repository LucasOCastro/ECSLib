namespace ECSLib.Extensions;

public static class CollectionExtension
{
    public static IEnumerable<T> Except<T>(this IEnumerable<T> col, T val) =>
        col.Where(obj => val != null && !val.Equals(obj));
}