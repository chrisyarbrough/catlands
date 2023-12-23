namespace CatLands;

using System.Reflection;

public sealed class FieldDescriptor : Descriptor<FieldInfo>
{
	public FieldDescriptor(FieldInfo info) : base(info)
	{
	}

	protected override string UnconventionalName
	{
		get
		{
			var attribute = Info.GetCustomAttribute<SerializeFieldAttribute>();

			if (attribute is { OverrideName: not null })
				return attribute.OverrideName;

			return Info.Name;
		}
	}

	public override bool CanWrite => !Info.IsInitOnly;

	public override Type Type => Info.FieldType;

	public override void Write(object target, object? value) => Info.SetValue(target, value);

	protected override object? GetPropertyValue(object target) => Info.GetValue(target);
}