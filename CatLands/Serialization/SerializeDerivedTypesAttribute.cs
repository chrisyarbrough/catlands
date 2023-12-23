namespace CatLands;

/// <summary>
/// Add this to a base type whose derives types should be serialized with type information
/// so that they can be restored to their specific type.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class SerializeDerivedTypesAttribute : Attribute
{
}