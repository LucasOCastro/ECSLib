namespace ECSLib;

/// <summary>
/// Records a field's old names so binary serialization doesn't break when a field is renamed.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class LegacyNameAttribute : Attribute
{
    public string[] LegacyNames { get; }

    public LegacyNameAttribute(params string[] names)
    {
        LegacyNames = names;
    }
}