namespace CatLands;

using System.Reflection;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

public sealed class CustomTypeInspector : ITypeInspector
{
	public bool IsDeserializing { get; init; }

	private static readonly BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	public IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
	{
		foreach (PropertyInfo property in type.GetProperties(flags))
		{
			if (property.CanWrite && !property.HasAttribute<DontSerializeAttribute>())
			{
				yield return new PropertyDescriptor(property);

				if (IsDeserializing)
				{
					// Handles renamed properties by pretending the object still has one of the old names.
					// Non-matching properties are ignored (configured on the DeserializedBuilder).
					foreach (var attribute in property.GetCustomAttributes<RenamedFromAttribute>())
					{
						yield return new PropertyDescriptor(property, attribute.OldName);
					}
				}
			}
		}

		foreach (FieldInfo field in type.GetFields(flags))
		{
			if (field.HasAttribute<SerializeFieldAttribute>())
				yield return new FieldDescriptor(field);
		}
	}

	public IPropertyDescriptor GetProperty(
		Type type,
		object? container,
		string name,
		bool ignoreUnmatched)
	{
		var candidates = GetProperties(type, container)
			.Where(p => string.Compare(p.Name, name, StringComparison.OrdinalIgnoreCase) == 0);

		using var enumerator = candidates.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			if (ignoreUnmatched)
			{
				return null!;
			}

			throw new SerializationException($"Property '{name}' not found on type '{type.FullName}'.");
		}

		var property = enumerator.Current;

		if (enumerator.MoveNext())
		{
			throw new SerializationException(
				$"Multiple properties with the name/alias '{name}' already exists on type '{type.FullName}', maybe you're misusing YamlAlias or maybe you are using the wrong naming convention? The matching properties are: {string.Join(", ", candidates.Select(p => p.Name).ToArray())}"
			);
		}

		return property;
	}
}