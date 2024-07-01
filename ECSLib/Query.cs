namespace ECSLib;

public readonly struct Query
{
    private readonly Type[] _all = [];
    private readonly Type[] _any = [];
    private readonly Type[] _none = [];
    
    internal bool HasAll => _all.Length > 0;
    internal IEnumerable<Type> GetAll() => _all;
    
    internal bool HasAny => _any.Length > 0;
    internal IEnumerable<Type> GetAny() => _any;
    
    internal bool HasNone => _none.Length > 0;
    internal IEnumerable<Type> GetNone() => _none;

    private Query(Type[] all, Type[] any, Type[] none)
    {
        _all = all;
        _any = any;
        _none = none;
    }

    public Query WithAll(params Type[] types) => new(types.ToArray(), _any, _none);
    public Query WithAny(params Type[] types) => new(_all, types.ToArray(), _none);
    public Query WithNone(params Type[] types) => new(_all, _any, types.ToArray());

    public static Query All(params Type[] types) => new(types.ToArray(), [], []);
    public static Query Any(params Type[] types) => new([], types.ToArray(), []);
    public static Query None(params Type[] types) => new([], [], types.ToArray());
}