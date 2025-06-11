namespace ECSLib.Benchmarks.OOPImplementation;

public class OOPEntity
{
    public int Health { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public void SetPosition(int x, int y) => (X, Y) = (x, y);
    
    public bool HasVelocity { get; set; }
    public int XVelocity { get; set; }
    public int YVelocity { get; set; }
    public void SetVelocity(int x, int y) => (HasVelocity, XVelocity, YVelocity) = (true, x, y);
    
    public bool HasBoundingBox { get; set; }
    public int MinX { get; set; }
    public int MinY { get; set; }
    public int MaxX { get; set; }
    public int MaxY { get; set; }

    public void SetBoundingBox(int minX, int minY, int maxX, int maxY) =>
        (HasBoundingBox, MinX, MinY, MaxX, MaxY) = (true, minX, minY, maxX, maxY);

    
    public void Update()
    {
        if (HasVelocity)
            ApplyVelocity();
        
        if (HasBoundingBox)
            ApplyBoundingBox();
    }

    private void ApplyVelocity()
    {
        X += XVelocity;
        Y += YVelocity;
    }
    
    private void ApplyBoundingBox()
    {
        if (X < MinX)
            X = MinX;
        if (X > MaxX)
            X = MaxX;
        if (Y < MinY)
            Y = MinY;
        if (Y > MaxY)
            Y = MaxY;
    }
}