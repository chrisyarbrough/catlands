using Raylib_cs;

public static class Cursor
{
	private static readonly Stack<MouseCursor> cursors = new();
	private static bool isDirty;

	static Cursor()
	{
		Push(MouseCursor.MOUSE_CURSOR_DEFAULT);
	}

	public static void Push(MouseCursor cursor)
	{
		cursors.Push(cursor);
		isDirty = true;
	}

	public static void Reset()
	{
		while (cursors.Count > 1)
		{
			cursors.Pop();
			isDirty = true;
		}
	}

	public static void Draw()
	{
		if (isDirty)
		{
			isDirty = false;
			Raylib.SetMouseCursor(cursors.Peek());
		}
	}
}