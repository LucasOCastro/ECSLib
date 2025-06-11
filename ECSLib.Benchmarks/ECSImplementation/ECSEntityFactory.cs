using System;
using System.Collections.Generic;
using ECSLib.Entities;

namespace ECSLib.Benchmarks.ECSImplementation;

public class ECSEntityFactory(ECS world) : IEntityFactory<Entity>
{
    private Health? _health;
    private Position? _position;
    private Velocity? _velocity;
    private BoundingBox? _boundingBox;

    public IEntityFactory<Entity> WithHealth(int hp)
    {
        _health = new() { Hp = hp };
        return this;
    }

    public IEntityFactory<Entity> WithPosition(int x, int y)
    {
        _position = new() { X = x, Y = y };
        return this;
    }

    public IEntityFactory<Entity> WithVelocity(int x, int y)
    {
        _velocity = new() { X = x, Y = y };
        return this;
    }

    public IEntityFactory<Entity> WithBoundingBox(int minX, int minY, int maxX, int maxY)
    {
        _boundingBox = new() { MinX = minX, MinY = minY, MaxX = maxX, MaxY = maxY };
        return this;
    }
    
    public Entity Build()
    {
        var e = world.CreateEntityWithComponents(GetActiveTypes());
        if (_health.HasValue) world.GetComponent<Health>(e) = _health.Value;
        if (_position.HasValue) world.GetComponent<Position>(e) = _position.Value;
        if (_velocity.HasValue) world.GetComponent<Velocity>(e) = _velocity.Value;
        if (_boundingBox.HasValue) world.GetComponent<BoundingBox>(e) = _boundingBox.Value;

        Clear();
        return e;
    }
    
    private IEnumerable<Type> GetActiveTypes()
    {
        if (_health.HasValue) yield return typeof(Health);
        if (_position.HasValue) yield return typeof(Position);
        if (_velocity.HasValue) yield return typeof(Velocity);
        if (_boundingBox.HasValue) yield return typeof(BoundingBox);
    }

    private void Clear()
    {
        _health = null;
        _position = null;
        _velocity = null;
        _boundingBox = null;
    }
}