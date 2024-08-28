// ReSharper disable StaticMemberInGenericType
namespace ECSLib.Components.Interning;

/// <summary>
/// Pool for reference values which are accessed via unique ID.
/// Uses the global context in <see cref="RefPoolContext"/>.
/// </summary>
 public static class RefPool<T> where T: class?
{
    private static readonly Dictionary<int, T> Pool = [];
    
    private static int _currentId;
    private static readonly Queue<int> ReleasedIds = [];

    public static T Get(int id) => Pool[id];
    public static void Set(int id, T value) => Pool[id] = value;
    
    /// <summary>
    /// Allocates a new space in the bool, associates it with an ID and returns it.
    /// <br/>
    /// ATTENTION: First you must initialize a context in <see cref="RefPoolContext"/>. 
    /// </summary>
    /// <param name="value">The initial value in the pool.</param>
    /// <returns>The pool ID assigned to the caller.</returns>
    /// <exception cref="Exception">
    /// Thrown if the global context in <see cref="RefPoolContext"/> wasn't initialized before calling.
    /// </exception>
    public static int Register(T value)
    {
        if (RefPoolContext.CurrentContext == null)
            throw new($"RefPool was instantiated before setting global context in {nameof(RefPoolContext)}.");
        
        int id = ReleasedIds.Count > 0 ? ReleasedIds.Dequeue() : _currentId++;
        Pool.Add(id, value);
        RefPoolContext.CurrentContext.RegisterReleaseId(id, Release);
        return id;
    }
    
    /// <summary>
    /// Releases the pool slot associated with the id, allowing this id to be used by another pooled reference.
    /// </summary>
    public static void Release(int id)
    {
        Pool.Remove(id);
        ReleasedIds.Enqueue(id);
    }
}