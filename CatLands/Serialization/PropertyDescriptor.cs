namespace CatLands;

using System.Reflection;
using System.Text.RegularExpressions;

public sealed class PropertyDescriptor : Descriptor<PropertyInfo>
{
	public PropertyDescriptor(PropertyInfo info) : base(info)
	{
	}

	public override string Name => Regex.Replace(Info.Name, "^<(.+?)>k__BackingField$", "$1");

	public override bool CanWrite => Info.CanWrite;

	public override Type Type => Info.PropertyType;

	public override void Write(object target, object? value) => Info.SetValue(target, value);

	protected override object? GetPropertyValue(object target) => Info.GetValue(target);
}