namespace ECSLib.Systems.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ECSSystemClassAttribute : Attribute
{
    public int Pipeline;
}