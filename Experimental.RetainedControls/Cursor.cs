using Raylib_cs;

public static class Cursor
{
	private static MouseCursor current = MouseCursor.MOUSE_CURSOR_DEFAULT;

	public static void Update(Gizmo hotControl, Gizmo hoveredControl)
	{
		MouseCursor cursorType = MouseCursor.MOUSE_CURSOR_DEFAULT;

		if (hotControl != null)
			cursorType = hotControl.GetMouseCursor();
		else if (hoveredControl != null)
			cursorType = hoveredControl.GetMouseCursor();

		Set(cursorType);
	}

	public static void Set(MouseCursor cursor)
	{
		if (current != cursor)
		{
			current = cursor;
			Raylib.SetMouseCursor(current);
		}
	}
}