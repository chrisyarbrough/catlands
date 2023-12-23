namespace CatLands;

using Newtonsoft.Json;

public class Layer
{
	public bool IsVisible
	{
		get => isVisible;
		set
		{
			if (value != isVisible)
			{
				isVisible = value;
				changeTracker?.NotifyChange();
			}
		}
	}

	public string Name => Path.GetFileNameWithoutExtension(TexturePath);
	public string TexturePath => texturePath;

	public IEnumerable<(Coord, int)> Tiles => tiles.Select(kvp => (kvp.Key, kvp.Value));

	private readonly string texturePath = string.Empty;

	private Dictionary<Coord, int> tiles = new();

	private ChangeTracker? changeTracker;
	private bool isVisible = true;

	[JsonConstructor]
	public Layer()
	{
	}
	
	public Layer(string texturePath) : this(texturePath, Enumerable.Empty<(Coord, int)>())
	{
	}

	public Layer(string texturePath, IEnumerable<(Coord, int)> tiles)
	{
		int assetsStartIndex = texturePath.IndexOf("Assets", StringComparison.Ordinal);
		if (assetsStartIndex != -1)
			texturePath = texturePath[assetsStartIndex..];

		this.texturePath = texturePath;
		this.tiles = tiles.ToDictionary(kvp => kvp.Item1, kvp => kvp.Item2);
	}

	public bool TryGetTileId(Coord coord, out int id)
	{
		return tiles.TryGetValue(coord, out id);
	}

	public void SetTileId(Coord coord, int id)
	{
		if (tiles.TryGetValue(coord, out int existingId) && existingId == id)
			return;

		tiles[coord] = id;
		changeTracker?.NotifyChange();
	}

	public void RemoveTile(Coord coord)
	{
		if (tiles.Remove(coord))
			changeTracker?.NotifyChange();
	}

	internal void SetChangeTracker(ChangeTracker changeTracker)
	{
		this.changeTracker = changeTracker ?? throw new ArgumentNullException(nameof(changeTracker));
	}
}