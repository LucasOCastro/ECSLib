using ECSLib.Entities;

namespace ECSLib;

public delegate void QueryAction(Entity entity);

public delegate void QueryAction<T1>(Entity entity, ref T1 a) 
    where T1 : struct;

public delegate void QueryAction<T1, T2>(Entity entity, ref T1 a, ref T2 b) 
    where T1 : struct
    where T2 : struct;

public delegate void QueryAction<T1, T2, T3>(Entity entity, ref T1 a, ref T2 b, ref T3 c) 
    where T1 : struct
    where T2 : struct
    where T3 : struct;

public delegate void QueryAction<T1, T2, T3, T4>(Entity entity, ref T1 a, ref T2 b, ref T3 c, ref T4 d) 
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct;

public delegate void QueryAction<T1, T2, T3, T4, T5>(Entity entity, ref T1 a, ref T2 b, ref T3 c, ref T4 d, ref T5 e) 
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
    where T5 : struct;

public delegate void QueryAction<T1, T2, T3, T4, T5, T6>(Entity entity, ref T1 a, ref T2 b, ref T3 c, ref T4 d, ref T5 e, ref T6 f) 
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
    where T5 : struct
    where T6 : struct;