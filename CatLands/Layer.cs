namespace CatLands;

using Newtonsoft.Json;

public class Layer
{
	public string Name => Path.GetFileNameWithoutExtension(TexturePath);
	public string TexturePath => texturePath;

	public IEnumerable<(Coord, int)> Tiles => tiles.Select(kvp => (kvp.Key, kvp.Value));

	[SerializeMember]
	private readonly string texturePath = string.Empty;

	[SerializeMember]
	private Dictionary<Coord, int> tiles = new();

	private ChangeTracker? changeTracker;

	[JsonConstructor]
	private Layer()
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

	public void Add(Coord coord, int i)
	{
		tiles.Add(coord, i);
	}

	// public IEnumerator<(Coord, int)> GetEnumerator()
	// {
	// 	foreach((Coord c, int i) in tiles)
	// 		yield return (c, i);
	// }
	//
	// IEnumerator IEnumerable.GetEnumerator()
	// {
	// 	return GetEnumerator();
	// }
}