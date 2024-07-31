using ECSLib.Components.Interning;

namespace ECSLib.XML.Tests;

public struct Components
{
    public float Speed = 1.0f;
    public bool CanRun = false;

    public Components()
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