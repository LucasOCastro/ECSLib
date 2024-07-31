// ReSharper disable StaticMemberInGenericType
namespace ECSLib.Components.Interning;

//TODO release stuff on entity destruction
public static class RefPool<T> where T: class?
{
    private static readonly Dictionary<int, T> Pool = [];
    
    private static int _currentId;
    private static readonly Queue<int> ReleasedIds = [];

    public static T Get(int id) => Pool[id];
    public static void Set(int id, T value) => Pool[id] = value;
    
    public static int Register(T value)
    {
        int id = ReleasedIds.Count > 0 ? ReleasedIds.Dequeue() : _currentId++;
        Pool.Add(id, value);
        return id;
    }
    
    public static void Release(int id)
    {
        Pool.Remove(id);
        ReleasedIds.Enqueue(id);
    }
}