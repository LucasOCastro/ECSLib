
namespace ECSLib.Extensions;

public static class DictionaryExtension
{
    /// <summary>
    /// If the dictionary does not contain <see cref="key"/>, insert <see cref="value"/> and return it.
    /// </summary>
    /// <returns>The stored value if present, otherwise <see cref="value"/>.</returns>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
    {
        if (!dict.TryGetValue(key, out var result))
        {
            result = value;
            dict.Add(key, result);
        }
        return result;
    }

    /// <summary>
    /// If the dictionary does not contain <see cref="key"/>,
    /// call <see cref="valueFunc"/>, insert its output and return it.
    /// </summary>
    /// <returns>The stored value if present, otherwise the return value of <see cref="valueFunc"/>.</returns>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> valueFunc)
    {
        if (!dict.TryGetValue(key, out var result))
        {
            result = valueFunc();
            dict.Add(key, result);
        }
        return result;
    }
    
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