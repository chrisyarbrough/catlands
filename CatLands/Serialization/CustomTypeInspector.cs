namespace CatLands;

using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

public sealed class CustomTypeInspector : TypeInspectorSkeleton
{
	private static readonly BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
	{
		foreach (PropertyInfo property in type.GetProperties(flags))
		{
			if (property.CanWrite && property.CanRead && !property.HasAttribute<DontSerialize>() &&
			    property.GetGetMethod(true)!.GetParameters().Length == 0)
				yield return new PropertyDescriptor(property);
		}

		foreach (FieldInfo field in type.GetFields(flags))
		{
			if (field.HasAttribute<SerializeFieldAttribute>())
				yield return new FieldDescriptor(field);
		}
	}
}