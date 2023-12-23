namespace CatLands.CatLands;

using System.Numerics;

[YamlTypeConverter(nameof(Instance))]
public class YamlVector2Converter : SingleLineComponentsConverter<Vector2>
{
	public static readonly YamlVector2Converter Instance = new();

	protected override Vector2 Create(string[] parts) => new(
		x: int.Parse(parts[0]),
		y: int.Parse(parts[1])
	);

	protected override string Serialize(Vector2 value) => $"{value.X} {value.Y}";
}