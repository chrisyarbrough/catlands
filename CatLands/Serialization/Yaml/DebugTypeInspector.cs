namespace CatLands;

using System.Runtime.Serialization;
using YamlDotNet.Serialization;

public sealed class DebugTypeInspector : ITypeInspector
{
	private readonly ITypeInspector inner;
	private readonly HashSet<string> propertyNames = new(StringComparer.OrdinalIgnoreCase);

	public DebugTypeInspector(ITypeInspector inner)
	{
		this.inner = inner;
	}

	public IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
	{
		foreach (IPropertyDescriptor descriptor in inner.GetProperties(type, container))
		{
			if (!propertyNames.Add(descriptor.Name))
			{
				throw new SerializationException(
					$"Duplicate property name '{descriptor.Name}' detected on type '{type.FullName}'." +
					$"This can happen when two serialized members differ only by case and is unsupported.");
			}

			yield return descriptor;
		}
	}

	public IPropertyDescriptor GetProperty(Type type, object? container, string name, bool ignoreUnmatched)
	{
		return inner.GetProperty(type, container, name, ignoreUnmatched);
	}
}