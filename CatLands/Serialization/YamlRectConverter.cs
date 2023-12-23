namespace CatLands;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

[YamlTypeConverter(nameof(Instance))]
public class YamlRectConverter : IYamlTypeConverter
{
	public static readonly YamlRectConverter Instance = new();

	public bool Accepts(Type type) => type == typeof(Rect);

	public object ReadYaml(IParser parser, Type type)
	{
		var scalar = parser.Consume<Scalar>();
		string[] parts = scalar.Value.Split(' ');

		if (parts.Length != 4)
		{
			throw new FormatException(
				$"The provided {nameof(Rect)} string must contain four components. Found: '{scalar.Value}'");
		}

		return new Rect(
			x: int.Parse(parts[0]),
			y: int.Parse(parts[1]),
			width: int.Parse(parts[2]),
			height: int.Parse(parts[3])
		);
	}

	public void WriteYaml(IEmitter emitter, object? value, Type type)
	{
		var rect = (Rect)value!;
		emitter.Emit(new Scalar($"{rect.X} {rect.Y} {rect.Width} {rect.Height}"));
	}
}