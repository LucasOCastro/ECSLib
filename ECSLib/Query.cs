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
    
    #region ALL
    
    public static Query With(params Type[] types) => new(types, [], []);
    public static Query With<T1>() => With(typeof(T1));
    public static Query With<T1, T2>() => With(typeof(T1), typeof(T2));
    public static Query With<T1, T2, T3>() => With(typeof(T1), typeof(T2), typeof(T3));
    public static Query With<T1, T2, T3, T4>() => With(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
    public static Query With<T1, T2, T3, T4, T5>() => With(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
    public static Query With<T1, T2, T3, T4, T5, T6>() => With(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
    
    #endregion
    
    #region ANY
    
    public Query WithAny(params Type[] types) => new(All, types, None);
    public Query WithAny<T1>() => WithAny(typeof(T1));
    public Query WithAny<T1, T2>() => WithAny(typeof(T1), typeof(T2));
    public Query WithAny<T1, T2, T3>() => WithAny(typeof(T1), typeof(T2), typeof(T3));
    public Query WithAny<T1, T2, T3, T4>() => WithAny(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
    public Query WithAny<T1, T2, T3, T4, T5>() => WithAny(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
    public Query WithAny<T1, T2, T3, T4, T5, T6>() => WithAny(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
    
    #endregion
    
    
    #region NONE
    
    public Query WithNone(params Type[] types) => new(All, Any, types);
    public Query WithNone<T1>() => WithNone(typeof(T1));
    public Query WithNone<T1, T2>() => WithNone(typeof(T1), typeof(T2));
    public Query WithNone<T1, T2, T3>() => WithNone(typeof(T1), typeof(T2), typeof(T3));
    public Query WithNone<T1, T2, T3, T4>() => WithNone(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
    public Query WithNone<T1, T2, T3, T4, T5>() => WithNone(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
    public Query WithNone<T1, T2, T3, T4, T5, T6>() => WithNone(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
    
    #endregion
}