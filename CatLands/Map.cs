namespace CatLands;

using System.Text;

public class Map
{
	public static Map? Current;
	
	public int Version = 1;
	public string[] Tilesets = Array.Empty<string>();
	public List<Tile> Tiles = new();

	public Map()
	{
	}

	public Map(Map map)
	{
		Version = map.Version;
		Tilesets = new string[map.Tilesets.Length];
		Tiles = new List<Tile>(map.Tiles);
		Array.Copy(map.Tilesets, Tilesets, map.Tilesets.Length);
	}

	public int Get(Coord gridPosition)
	{
		Tile? tile = Tiles.FirstOrDefault(x => x.Coord == gridPosition);
		if (tile != null)
			return tile.Id;

		return -1;
	}

	public void Set(Coord coord, int tileId, int tilesetId)
	{
		Tile? tile = Tiles.FirstOrDefault(tile => tile.Coord == coord);
		if (tile != null)
			tile.Id = tileId;
		else
		{
			Tiles.Add(new Tile
			{
				Coord = coord,
				Id = tileId,
				TilesetId = tilesetId
			});
		}
	}

	public void Remove(Coord gridPosition)
	{
		Tiles.RemoveAll(x => x.Coord == gridPosition);
	}

	public override string ToString()
	{
		var sb = new StringBuilder();
		sb.Append("Version: ").AppendLine(Version.ToString());
		foreach (var tileset in Tilesets)
			sb.AppendLine(tileset);
		foreach (var tile in Tiles)
			sb.AppendLine(tile.ToString());
		return sb.ToString();
	}
}

public class Tile
{
	public int Id;
	public int TilesetId;
	public Coord Coord;

	public override string ToString()
	{
		return $"Tile {Id} ({Coord})";
	}
}