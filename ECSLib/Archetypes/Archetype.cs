using ECSLib.Components;

namespace ECSLib.Archetypes;

/// <summary>
/// Represents an archetype as its definition and the component arrays it contains.
/// </summary>
internal readonly struct Archetype
{
    public ArchetypeDefinition Definition { get; }
    public ComponentCollectionSet Components { get; }
    
    public Archetype(ArchetypeDefinition definition)
    {
        Definition = definition;
        Components = new(definition.Components);
    }
}