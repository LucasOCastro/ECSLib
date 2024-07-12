using ECSLib.Components;
using ECSLib.Entities;

namespace ECSLib;

public delegate void QueryAction(Entity entity);

public delegate void QueryAction<T1>(Entity entity, ref Comp<T1> comp1)
    where T1 : struct;

public delegate void QueryAction<T1, T2>(Entity entity, ref Comp<T1> comp1, ref Comp<T2> comp2) 
    where T1 : struct
    where T2 : struct;

public delegate void QueryAction<T1, T2, T3>(Entity entity, ref Comp<T1> comp1, ref Comp<T2> comp2, ref Comp<T3> comp3) 
    where T1 : struct
    where T2 : struct
    where T3 : struct;

public delegate void QueryAction<T1, T2, T3, T4>(Entity entity, ref Comp<T1> comp1, ref Comp<T2> comp2, ref Comp<T3> comp3, ref Comp<T4> comp4) 
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct;

public delegate void QueryAction<T1, T2, T3, T4, T5>(Entity entity, ref Comp<T1> comp1, ref Comp<T2> comp2, ref Comp<T3> comp3, ref Comp<T4> comp4, ref Comp<T5> comp5) 
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
    where T5 : struct;

public delegate void QueryAction<T1, T2, T3, T4, T5, T6>(Entity entity, ref Comp<T1> comp1, ref Comp<T2> comp2, ref Comp<T3> comp3, ref Comp<T4> comp4, ref Comp<T5> comp5, ref Comp<T6> comp6)
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
    where T5 : struct
    where T6 : struct;