namespace ECSLib.Components.Interning;

//TODO deal with the nullability issue
//TODO release stuff on entity destruction
public static class RefPool<T> where T: class
{
    private static readonly Dictionary<int, T?> _pool = [];
    
    private static int _currentId;
    private static readonly Queue<int> _releasedIds = [];

    public static T? Get(int id) => _pool[id];
    public static void Set(int id, T? value) => _pool[id] = value;
    
    public static int Register()
    {
        int id = _releasedIds.Count > 0 ? _releasedIds.Dequeue() : _currentId++;
        _pool.Add(id, default);
        return id;
    }
    
    public static void Release(int id)
    {
        _pool.Remove(id);
        _releasedIds.Enqueue(id);
    }
}