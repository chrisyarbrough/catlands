namespace CatLands;

public class Map
{
	public int Version = 1;
	public string[] Tilesets = Array.Empty<string>();
	public List<Tile> Tiles = new();

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

public struct Coord
{
	public int X;
	public int Y;

	public Coord(int x, int y)
	{
		X = x;
		Y = y;
	}

	public override string ToString()
	{
		return $"{X}|{Y}";
	}

	public bool Equals(Coord other)
	{
		return X == other.X && Y == other.Y;
	}

	public override bool Equals(object? obj)
	{
		return obj is Coord other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(X, Y);
	}

	public static bool operator ==(Coord a, Coord b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(Coord a, Coord b)
	{
		return !a.Equals(b);
	}
}