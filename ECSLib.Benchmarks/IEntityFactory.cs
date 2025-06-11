namespace ECSLib.Benchmarks;

public interface IEntityFactory<out T>
{
    IEntityFactory<T> WithHealth(int hp);
    IEntityFactory<T> WithPosition(int x, int y);
    IEntityFactory<T> WithVelocity(int x, int y);
    IEntityFactory<T> WithBoundingBox(int minX, int minY, int maxX, int maxY);
    T Build();
}