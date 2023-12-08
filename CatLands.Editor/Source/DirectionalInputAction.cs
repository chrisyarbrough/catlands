namespace CatLands.Editor;

using Raylib_cs;

public class DirectionalInputAction
{
	private readonly Dictionary<KeyboardKey, Coord> keyToCoord = new()
	{
		{ KeyboardKey.KEY_UP, new Coord(0, -1) },
		{ KeyboardKey.KEY_W, new Coord(0, -1) },

		{ KeyboardKey.KEY_DOWN, new Coord(0, 1) },
		{ KeyboardKey.KEY_S, new Coord(0, 1) },

		{ KeyboardKey.KEY_LEFT, new Coord(-1, 0) },
		{ KeyboardKey.KEY_A, new Coord(-1, 0) },

		{ KeyboardKey.KEY_RIGHT, new Coord(1, 0) },
		{ KeyboardKey.KEY_D, new Coord(1, 0) },
	};

	private readonly RepeatAction repeatAction = new();
	private KeyboardKey? currentKey;

	public bool Begin(out Coord coord)
	{
		foreach (KeyboardKey key in keyToCoord.Keys)
		{
			if (Raylib.IsKeyPressed(key))
			{
				currentKey = key;
				repeatAction.Begin();
				coord = keyToCoord[currentKey.Value];
				return true;
			}
			if (Raylib.IsKeyReleased(key))
			{
				currentKey = null;
				repeatAction.End();
			}
		}

		if (currentKey != null)
		{
			float deltaTime = Raylib.GetFrameTime();
			coord = keyToCoord[currentKey.Value];
			return repeatAction.Update(deltaTime);
		}

		coord = default;
		return false;
	}
}