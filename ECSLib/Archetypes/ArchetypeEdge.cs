namespace ECSLib.Archetypes;

/// <summary>
/// Represents the edge between to archetypes indicating a difference which is an added or removed component type. 
/// </summary>
/// <param name="Add">The component type added, or null if the component was removed instead.</param>
/// <param name="Remove">The component type removed, or null if the component was added instead.</param>
internal readonly record struct ArchetypeEdge(Type? Add, Type? Remove);