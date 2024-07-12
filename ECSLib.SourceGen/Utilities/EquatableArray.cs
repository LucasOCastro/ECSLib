using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ECSLib.SourceGen.Utilities;

public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
{
    private readonly T[] _array;

    public EquatableArray(IEnumerable<T> array)
    {
        _array = array.ToArray();
    }
    
    public T this[int index] => _array[index];

    public int Length => _array.Length;

    public bool Equals(EquatableArray<T> other)
    {
        if (_array.Length != other._array.Length)
            return false;

        for (int i = 0; i < _array.Length; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(_array[i], other._array[i]))
                return false;
        }
        return true;
    }
    
    public override bool Equals(object? obj) => obj is EquatableArray<T> arr && Equals(arr);

    public override int GetHashCode()
    {
        unchecked
        {
            const int seed = 487;
            const int modifier = 31;

            int hash = seed;
            foreach (var item in _array)
            {
                hash = hash * modifier + (item?.GetHashCode() ?? 0);
            }
            return hash;
        }
    }

    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);

    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !(left == right);


    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_array).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    
}