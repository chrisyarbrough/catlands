namespace CatLands.SpriteEditor;

public static class Gui
{
	private static Stack<bool> changedStack = new();

	public static void BeginChangeCheck()
	{
		changedStack.Push(false);
	}

	internal static void SetChanged()
	{
		if (changedStack.Count > 0)
		{
			changedStack.Pop();
			changedStack.Push(true);
		}
	}

	public static bool EndChangeCheck()
	{
		return changedStack.Pop();
	}
}