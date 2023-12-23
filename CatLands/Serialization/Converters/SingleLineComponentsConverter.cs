namespace CatLands;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

public abstract class SingleLineComponentsConverter<T> : IYamlTypeConverter
{
	public bool Accepts(Type type) => type == typeof(T);

	public object ReadYaml(IParser parser, Type type)
	{
		var scalar = parser.Consume<Scalar>();
		string[] parts = scalar.Value.Split(' ');

		if (parts.Length != ComponentsCount)
		{
			throw new FormatException(
				$"The provided {nameof(T)} string must contain {ComponentsCount} components. Found: '{scalar.Value}'");
		}

		return Create(parts)!;
	}

	protected virtual int ComponentsCount => 2;

	public void WriteYaml(IEmitter emitter, object? value, Type type)
	{
		T vector = (T)value!;
		emitter.Emit(new Scalar(Serialize(vector)));
	}

	protected abstract T Create(string[] parts);

	protected abstract string Serialize(T value);
}