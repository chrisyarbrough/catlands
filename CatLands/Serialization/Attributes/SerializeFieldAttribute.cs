namespace CatLands;

[AttributeUsage(AttributeTargets.Field)]
public class SerializeFieldAttribute : Attribute
{
	public readonly string? OverrideName;

	public SerializeFieldAttribute(string? name = null)
	{
		this.OverrideName = name;
	}
}