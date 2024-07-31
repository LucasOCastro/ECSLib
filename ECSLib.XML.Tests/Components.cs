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