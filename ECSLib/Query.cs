namespace ECSLib;

public readonly struct Query
{
    internal readonly Type[] All = [];
    internal readonly Type[] Any = [];
    internal readonly Type[] None = [];
    
    private Query(Type[] all, Type[] any, Type[] none)
    {
        All = all;
        Any = any;
        None = none;
    }
    
    public static Query With(params Type[] types) => new(types, [], []);
    public Query WithAny(params Type[] types) => new(All, types, None);
    public Query WithNone(params Type[] types) => new(All, Any, types);
}