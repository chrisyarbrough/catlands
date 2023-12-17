namespace CatLands.SpriteEditor;

public static class ListExtensions
{
	public static int ClampedIndex<T>(this IList<T> list, int index)
	{
		if (list.Count == 0)
			return -1;

		return Math.Clamp(index, 0, list.Count - 1);
	}
}