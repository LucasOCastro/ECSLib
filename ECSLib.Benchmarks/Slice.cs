using System.Diagnostics.Contracts;

namespace ECSLib.Benchmarks;

public record struct Slice(int Start, int Size)
{
    public int End => Start + Size;
            
    [Pure]
    public bool Contains(int index) => Start <= index && index < End;
}