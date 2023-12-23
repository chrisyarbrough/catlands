namespace CatLands;

/// <summary>
/// Use this on types that derive from a type with the <see cref="SerializeDerivedTypesAttribute"/> to indicate
/// the old name of the type after its namespace-qualified name was changed.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class RenamedFromAttribute : Attribute
{
	public readonly string OldName;

	/// <summary>
	/// Pass the name of this type before the renaming/move, e.g. <c>CatLands.Tests.SerializerTests+ChildComponentB</c>.
	/// </summary>
	/// <param name="oldName">The System.Type.FullName in the format Namespace.TypeName+NestedTypeName.</param>
	public RenamedFromAttribute(string oldName)
	{
		this.OldName = oldName;
	}
}