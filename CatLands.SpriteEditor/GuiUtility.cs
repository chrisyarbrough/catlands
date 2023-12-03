namespace CatLands.SpriteEditor;

public static class GuiUtility
{
	public static int? HotControl { get; set; }

	private static int nextControlId;

	public static int GetControlId()
	{
		return ++nextControlId;
	}

	public static void ResetControlId()
	{
		nextControlId = 0;
	}
}