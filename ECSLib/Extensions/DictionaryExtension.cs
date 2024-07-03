
namespace ECSLib.Extensions;

public static class DictionaryExtension
{
    /// <summary>
    /// If the dictionary does not contain <see cref="key"/>,
    /// constructs a new instance of <see cref="TValue"/>, insert it and return it.
    /// </summary>
    /// <returns>The stored value if present, otherwise a new instance of <see cref="TValue"/>.</returns>
    public static TValue GetOrAddNew<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue: new()
    {
        if (!dict.TryGetValue(key, out var result))
        {
            result = new();
            dict.Add(key, result);
        }
        return result;
    }
}