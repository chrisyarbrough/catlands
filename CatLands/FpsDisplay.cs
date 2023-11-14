using Raylib_cs;

public static class FpsDisplay
{
	private static bool enabled;

	public static void Update()
	{
		if (Raylib.IsKeyPressed(KeyboardKey.KEY_R) &&
		    (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SUPER) || Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL)))
		{
			enabled = !enabled;
		}

		if (enabled)
		{
			Raylib.DrawText(Raylib.GetFPS().ToString(), 12, 30, 20, Color.BLACK);
		}
	}
}