namespace CatLands;

using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeResolvers;

public abstract class Descriptor<T> : IPropertyDescriptor where T : MemberInfo
{
	protected readonly T Info;

	private readonly ITypeResolver typeResolver = new DynamicTypeResolver();

	protected Descriptor(T info)
	{
		this.Info = info;
	}

	public abstract string Name { get; }

	public abstract bool CanWrite { get; }

	public abstract Type Type { get; }

	public Type? TypeOverride { get; set; }

	public int Order { get; set; }

	public ScalarStyle ScalarStyle { get; set; } = ScalarStyle.Any;

	public T? GetCustomAttribute<T>() where T : Attribute => Info.GetCustomAttribute<T>();

	public IObjectDescriptor Read(object target)
	{
		object? propertyValue = GetPropertyValue(target);
		var actualType = TypeOverride ?? typeResolver.Resolve(Type, propertyValue);
		return new ObjectDescriptor(propertyValue, actualType, Type, ScalarStyle);
	}

	public abstract void Write(object target, object? value);

	protected abstract object? GetPropertyValue(object target);
}