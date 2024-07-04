namespace ECSLib.Archetypes;

/// <summary>
/// Represents the exact position an entity occupies in storage,
/// including the index of its archetype and the internal index within that archetype.
/// </summary>
internal readonly record struct ArchetypeRecord(int ArchetypeIndex, int EntityIndexInArchetype);