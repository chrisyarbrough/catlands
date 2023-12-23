namespace CatLands;

using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.TypeResolvers;

public abstract class Descriptor<TMemberInfo> : IPropertyDescriptor where TMemberInfo : MemberInfo
{
	protected readonly TMemberInfo Info;

	private readonly ITypeResolver typeResolver = new DynamicTypeResolver();

	protected Descriptor(TMemberInfo info)
	{
		Info = info;
	}

	public string Name => CamelCaseNamingConvention.Instance.Apply(UnconventionalName);
	
	protected abstract string UnconventionalName { get; }

	public abstract bool CanWrite { get; }

	public abstract Type Type { get; }

	public Type? TypeOverride { get; set; }

	public int Order { get; set; }

	public ScalarStyle ScalarStyle { get; set; } = ScalarStyle.Any;

	public TAttribute? GetCustomAttribute<TAttribute>() where TAttribute : Attribute => Info.GetCustomAttribute<TAttribute>();

	public IObjectDescriptor Read(object target)
	{
		object? propertyValue = GetPropertyValue(target);
		var actualType = TypeOverride ?? typeResolver.Resolve(Type, propertyValue);
		return new ObjectDescriptor(propertyValue, actualType, Type, ScalarStyle);
	}

	public abstract void Write(object target, object? value);

	protected abstract object? GetPropertyValue(object target);
}