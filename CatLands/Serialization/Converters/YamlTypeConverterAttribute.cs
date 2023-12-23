namespace CatLands;

using System.Reflection;
using YamlDotNet.Serialization;

/// <summary>
/// Add this to a type that implements <see cref="IYamlTypeConverter"/> to auto-register it with the serializer.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class YamlTypeConverterAttribute : Attribute
{
	public readonly string? InstanceFieldName;

	/// <summary>
	/// By default, a new instance of the converter is created whenever the serializer runs.
	/// If you want to use a singleton instance, specify the name of a field on the type which holds that instance.
	/// </summary>
	/// <param name="instanceFieldName">
	/// Must be a static field which holds an instance that is assignable to <see cref="IYamlTypeConverter"/>. 
	/// </param>
	public YamlTypeConverterAttribute(string? instanceFieldName = null)
	{
		InstanceFieldName = instanceFieldName;
	}

	public bool TryGetInstanceFieldValue(Type type, out IYamlTypeConverter converter)
	{
		if (InstanceFieldName == null)
		{
			converter = default!;
			return false;
		}

		FieldInfo? field = type.GetField(InstanceFieldName,
			BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

		if (field == null)
		{
			Throw(type, "could not be found. It must be static.");
		}

		if (!typeof(IYamlTypeConverter).IsAssignableFrom(field!.FieldType))
		{
			Throw(type, $"must be assignable to '{nameof(IYamlTypeConverter)}'.");
		}

		var fieldValue = (IYamlTypeConverter?)field.GetValue(null);

		if (fieldValue == null || fieldValue.GetType() != type)
		{
			Throw(type, $"must hold an instance of '{type.FullName}'.");
		}

		converter = fieldValue!;
		return true;
	}

	private void Throw(Type type, string reason)
	{
		throw new InvalidOperationException(
			$"The field '{InstanceFieldName}' on type '{type.FullName}' {reason}");
	}
}