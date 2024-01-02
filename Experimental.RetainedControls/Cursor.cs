using Raylib_cs;

public static class Cursor
{
	private static MouseCursor current = MouseCursor.MOUSE_CURSOR_DEFAULT;

	public static void SetFromGizmo(Gizmo hovered, Gizmo hot)
	{
		MouseCursor cursor = GetCursor(hovered, hot);
		Set(cursor);
	}

	private static MouseCursor GetCursor(Gizmo hovered, Gizmo hot)
	{
		if (hot is { HotCursor: not null })
			return hot.HotCursor.Value;

		if (hovered is { HoverCursor: not null })
			return hovered.HoverCursor.Value;

		return MouseCursor.MOUSE_CURSOR_DEFAULT;
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