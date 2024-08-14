namespace ECSLib.Extensions;

/// <summary>
/// Represents a collection of bidirectional (a&gt;&lt;b) pairs.
/// </summary>
public class BidirectionalMap<T1, T2> 
    where T1 : notnull 
    where T2 : notnull
{
    private readonly Dictionary<T1, T2> _forward = new();
    private readonly Dictionary<T2, T1> _reverse = new();

    public IEnumerable<T1> A => _forward.Keys;
    
    public IEnumerable<T2> B => _reverse.Keys;

    public IEnumerable<(T1, T2)> All => _forward.Select(p => (p.Key, p.Value)); 

    public T2 this[T1 key] => _forward[key];
    public T1 this[T2 key] => _reverse[key];
    
    /// <summary>
    /// Registers a bidirectional pair of "<see cref="a"/> to <see cref="b"/>" and "<see cref="b"/> to <see cref="a"/>".
    /// </summary>
    /// <exception cref="ArgumentException"><see cref="a"/> or <see cref="b"/> are already registered.</exception>
    public void Add(T1 a, T2 b)
    {
        _forward.Add(a, b);
        _reverse.Add(b, a);
    }

    /// <summary>
    /// If <see cref="a"/> is already associated with a value, replace it with <see cref="b"/>.<br/>
    /// If <see cref="b"/> is already associated with a value, replace it with <see cref="a"/>.<br/>
    /// Otherwise, simply add the connection.
    /// </summary>
    public void Set(T1 a, T2 b)
    {
        if (_forward.TryGetValue(a, out var currentValForA))
        {
            if (b.Equals(currentValForA)) return;
            _reverse.Remove(currentValForA);
            _forward[a] = b;
        } else _forward.Add(a, b);


        if (_reverse.TryGetValue(b, out var currentValForB))
        {
            if (a.Equals(currentValForB)) return;
            _forward.Remove(currentValForB);
            _reverse[b] = a;
        } else _reverse.Add(b, a);
    }
    
    /// <inheritdoc cref="Dictionary{TKey,TValue}.TryGetValue"/>
    public bool TryGet(T1 key, out T2? result) => _forward.TryGetValue(key, out result);
    
    /// <inheritdoc cref="Dictionary{TKey,TValue}.TryGetValue"/>
    public bool TryGet(T2 key, out T1? result) => _reverse.TryGetValue(key, out result);

    /// <returns>true if any bidirectional pair contains <see cref="value"/>, otherwise false.</returns>
    public bool Contains(T1 value) => _forward.ContainsKey(value);
    
    /// <inheritdoc cref="Contains(T1)"/>
    public bool Contains(T2 value) => _reverse.ContainsKey(value);
    
    /// <returns>true if there is a specific bidirectional pair (<see cref="a"/>&gt;&lt;<see cref="b"/>), otherwise false.</returns>
    public bool Contains(T1 a, T2 b) => TryGet(a, out var value) && b.Equals(value);

    /// <summary> Removes any bidirectional pair which contains <see cref="value"/>. </summary>
    public void Remove(T1 value)
    {
        _reverse.Remove(_forward[value]);
        _forward.Remove(value);
    }
    
    /// <inheritdoc cref="Remove(T1)"/>>
    public void Remove(T2 value)
    {
        _forward.Remove(_reverse[value]);
        _reverse.Remove(value);
    }
}