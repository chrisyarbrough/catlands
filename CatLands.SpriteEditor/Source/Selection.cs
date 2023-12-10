namespace CatLands.SpriteEditor;

public static class Selection
{
	private static readonly HashSet<int> ids = new();

	public static void AddToSelection(int id)
	{
		ids.Add(id);
	}

	public static void ClearSelection()
	{
		ids.Clear();
	}

	public static void SetSingleSelection(int id)
	{
		ids.Clear();
		ids.Add(id);
	}

	public static bool IsSelected(int id)
	{
		return ids.Contains(id);
	}

	public static bool HasSelection()
	{
		return ids.Count > 0;
	}

	public static int GetSingleSelection()
	{
		return ids.Count > 0 ? ids.First() : -1;
	}

	public static IEnumerable<int> GetSelection()
	{
		return ids;
	}
}