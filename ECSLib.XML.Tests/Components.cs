using ECSLib.Components.Interning;

namespace ECSLib.XML.Tests;

public struct MoverComponent
{
    public float Speed = 1.0f;
    public bool CanRun = false;

    public MoverComponent()
    {
    }
}

public struct HealthComponent
{
    public int Health = 5;
    public int DeathSound = -1;
    public PooledRef<string> DeathSpeech = new("Silent");

    public HealthComponent()
    {
    }
}

public struct DialogueComponent
{
    public PooledRef<List<int>> PricesList = new([1, 2, 3]);
    public PooledRef<float[]> Array = new([-1.56f]);
    public PooledRef<HashSet<ulong>> Set = new([]);
    public PooledRef<string[]> Dialogues = new(["Default Dialogue"]);
    public PooledRef<Dictionary<int, string>> Dict = new([]);

    public DialogueComponent()
    {
    }
}