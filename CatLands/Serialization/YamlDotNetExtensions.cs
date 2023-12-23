namespace CatLands;

using System.Reflection;
using YamlDotNet.Serialization;

public static class YamlDotNetExtensions
{
	public static SerializerBuilder WithAnnotatedTagMappings(this SerializerBuilder builder)
	{
		foreach ((string tagName, Type type) in FindMappings())
		{
			builder.WithTagMapping(tagName, type);
		}

		return builder;
	}

	private static IEnumerable<(string tagName, Type type)> FindMappings()
	{
		foreach (Type type in FindTypesWithAttribute<SerializeDerivedTypesAttribute>())
		{
			// Intentional simplification: A serialized type hierarchy must be in the same assembly.
			foreach (Type derivedType in type.Assembly.GetTypes().Where(t => t.IsSubclassOf(type)))
			{
				// Each type must resolve to a single tag during serialization.
				string tagName = CreateTagName(derivedType.FullName);
				yield return (tagName, derivedType);
			}
		}
	}

	public static DeserializerBuilder WithAnnotatedTagMappings(this DeserializerBuilder builder)
	{
		foreach ((string tagName, Type type) in FindMappings())
		{
			builder.WithTagMapping(tagName, type);
			
			// Multiple tags can resolve to the same type during deserialization.
			foreach (var attribute in type.GetCustomAttributes<RenamedFromAttribute>())
			{
				string oldTagName = CreateTagName(attribute.OldName);
				builder.WithTagMapping(oldTagName, type);
			}
		}

		return builder;
	}

	private static string CreateTagName(string? typeIdentifier)
	{
		// Create unique identifier for the type. However, we must remove the '+' symbol that is used
		// to indicate nested types in C# because YAML doesn't allow it in tags.
		return $"!{typeIdentifier}".Replace("+", "::");
	}

	public static T WithAnnotatedTypeConverters<T>(this BuilderSkeleton<T> builder) where T : BuilderSkeleton<T>
	{
		foreach (Type type in FindTypesWithAttribute<YamlTypeConverterAttribute>())
		{
			if (typeof(IYamlTypeConverter).IsAssignableFrom(type))
			{
				RegisterTypeConverter(builder, type);
			}
			else
			{
				throw new InvalidOperationException(
					$"{nameof(YamlTypeConverterAttribute)} can only be applied to a type that implements {nameof(IYamlTypeConverter)}.");
			}
		}

		return (T)builder;
	}

	private static void RegisterTypeConverter<T>(BuilderSkeleton<T> builder, Type type) where T : BuilderSkeleton<T>
	{
		var attribute = type.GetCustomAttribute<YamlTypeConverterAttribute>()!;
		if (attribute.TryGetInstanceFieldValue(type, out IYamlTypeConverter converter))
		{
			builder.WithTypeConverter(converter);
		}
		else
		{
			builder.WithTypeConverter((IYamlTypeConverter)Activator.CreateInstance(type)!);
		}
	}

	private static IEnumerable<Type> FindTypesWithAttribute<T>()
	{
		return AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(assembly => assembly.GetTypes())
			.Where(type => type.IsDefined(typeof(T)));
	}
}