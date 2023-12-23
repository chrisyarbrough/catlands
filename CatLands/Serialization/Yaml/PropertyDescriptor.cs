namespace CatLands;

using System.Reflection;
using System.Text.RegularExpressions;

public sealed class PropertyDescriptor : Descriptor<PropertyInfo>
{
	private readonly string? overrideName;

	public PropertyDescriptor(PropertyInfo info) : base(info)
	{
	}

	public PropertyDescriptor(PropertyInfo info, string overrideName) : base(info)
	{
		this.overrideName = overrideName;
	}

	protected override string UnconventionalName
	{
		get
		{
			if (overrideName != null)
				return overrideName;

			return Regex.Replace(Info.Name, "^<(.+?)>k__BackingField$", "$1");
		}
	}

	public override bool CanWrite => Info.CanWrite;

	public override Type Type => Info.PropertyType;

	public override void Write(object target, object? value) => Info.SetValue(target, value);

	protected override object? GetPropertyValue(object target) => Info.GetValue(target);
}