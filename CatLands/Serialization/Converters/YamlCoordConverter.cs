namespace CatLands;

[YamlTypeConverter(nameof(Instance))]
public class YamlCoordConverter : SingleLineComponentsConverter<Coord>
{
	public static readonly YamlCoordConverter Instance = new();

	protected override Coord Create(string[] parts) => new(
		x: int.Parse(parts[0]),
		y: int.Parse(parts[1])
	);

	protected override string Serialize(Coord value) => $"{value.X} {value.Y}";
}