namespace ECSLib.Systems;

public abstract class BaseSystem
{
    public abstract void Process(float dt, ECS world);
}