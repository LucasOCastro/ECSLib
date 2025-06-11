namespace ECSLib.Benchmarks.OOPImplementation;

public class OOPEntityFactory : IEntityFactory<OOPEntity>
{
    private OOPEntity _entity = new();
    
    public IEntityFactory<OOPEntity> WithHealth(int hp)
    {
        _entity.Health = hp;
        return this;
    }

    public IEntityFactory<OOPEntity> WithPosition(int x, int y)
    {
        _entity.SetPosition(x, y);
        return this;
    }

    public IEntityFactory<OOPEntity> WithVelocity(int x, int y)
    {
        _entity.SetVelocity(x, y);
        return this;
    }

    public IEntityFactory<OOPEntity> WithBoundingBox(int minX, int minY, int maxX, int maxY)
    {
        _entity.SetBoundingBox(minX, minY, maxX, maxY);
        return this;
    }

    public OOPEntity Build()
    {
        var entity = _entity;
        _entity = new();
        return entity;
    }
}