namespace CatLands.Editor;

public class MapDirtyTracker
{
	private readonly Map map;
	private readonly Map cleanMap;
	private static MapValueComparer comparer = new MapValueComparer();

	public MapDirtyTracker(Map map)
	{
		this.map = map;
		this.cleanMap = new Map(map);
	}

	public bool IsDirty()
	{
		return !comparer.Equals(map, cleanMap);
	}
}

public class MapValueComparer : IEqualityComparer<Map>
{
	public bool Equals(Map? x, Map? y)
	{
		if (ReferenceEquals(x, y))
			return true;
		if (ReferenceEquals(x, null))
			return false;
		if (ReferenceEquals(y, null))
			return false;
		if (x.GetType() != y.GetType())
			return false;
		return x.Version == y.Version && x.Tilesets.SequenceEqual(y.Tilesets) && x.Tiles.SequenceEqual(y.Tiles);
	}

	public int GetHashCode(Map obj)
	{
		return HashCode.Combine(obj.Version, obj.Tilesets, obj.Tiles);
	}
}