using CatLands.SpriteEditor;
using Raylib_cs;

public static class SaveDirtyTracker
{
	private static string? savedJson;

	public static void MarkClean(SpriteAtlas spriteAtlas)
	{
		Raylib.SetWindowTitle(Program.Title);
		savedJson = spriteAtlas.GetMemento();
	}

	public static void MarkDirty()
	{
		Raylib.SetWindowTitle(Program.Title + "*");
	}

	public static void EvaluateDirty(SpriteAtlas spriteAtlas)
	{
		if (savedJson != spriteAtlas.GetMemento())
			Raylib.SetWindowTitle(Program.Title + "*");
		else
			Raylib.SetWindowTitle(Program.Title);
	}
}