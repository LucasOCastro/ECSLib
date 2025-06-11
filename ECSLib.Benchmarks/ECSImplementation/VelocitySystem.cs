using ECSLib.Components;
using ECSLib.Entities;
using ECSLib.Systems;
using ECSLib.Systems.Attributes;

namespace ECSLib.Benchmarks.ECSImplementation;

[ECSSystemClass]
public partial class VelocitySystem : BaseSystem
{
    [ECSSystem]
    public static void ApplyVelocitySystem(ref Position position, in Velocity velocity, in Comp<BoundingBox> boundingBox)
    {
        position.X += velocity.X;
        position.Y += velocity.Y;
    }
    
    [ECSSystem]
    public static void LimitPositionSystem(in Entity entity, ref Position position, in BoundingBox boundingBox)
    {
        if (position.X < boundingBox.MinX)
            position.X = boundingBox.MinX;
        if (position.X > boundingBox.MaxX)
            position.X = boundingBox.MaxX;
        if (position.Y < boundingBox.MinY)
            position.Y = boundingBox.MinY;
        if (position.Y > boundingBox.MaxY)
            position.Y = boundingBox.MaxY;
    }
}