namespace CatLands;

using System.Reflection;
using YamlDotNet.Serialization;

/// <summary>
/// Add this to a base type whose derives types should be serialized with type information
/// so that they can be restored to their specific type.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class SerializeDerivedTypesAttribute : Attribute
{
}

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
}

public static class YamlDotNetExtensions
{
	public static T WithAnnotatedTagMappings<T>(this BuilderSkeleton<T> builder) where T : BuilderSkeleton<T>
	{
		foreach (Type type in FindTypesWithAttribute<SerializeDerivedTypesAttribute>())
		{
			// Intentional simplification: A serialized type hierarchy must be in the same assembly.
			foreach (Type derivedType in type.Assembly.GetTypes().Where(t => t.IsSubclassOf(type)))
			{
				// Create unique identifier for the type. However, we must remove the '+' symbol that is used
				// to indicate nested types in C# because YAML doesn't allow it in tags.
				string tagName = $"!{derivedType.FullName}".Replace("+", "::");

				builder.WithTagMapping(tagName, derivedType);
			}
		}

		return (T)builder;
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
		if (attribute.InstanceFieldName != null)
		{
			FieldInfo? field = type.GetField(attribute.InstanceFieldName,
				BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

			if (field == null)
			{
				throw new InvalidOperationException(
					$"The static field '{attribute.InstanceFieldName}' on type '{type.FullName}' could not be found.");
			}

			if (!typeof(IYamlTypeConverter).IsAssignableFrom(field.FieldType))
			{
				throw new InvalidOperationException(
					$"The field '{attribute.InstanceFieldName}' on type '{type.FullName}' must be assignable to '{nameof(IYamlTypeConverter)}'.");
			}

			builder.WithTypeConverter((IYamlTypeConverter)field.GetValue(null)!);
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