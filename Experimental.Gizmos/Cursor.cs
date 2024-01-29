namespace Experimental.Gizmos;

using ImGuiNET;
using Raylib_cs;

public static class Cursor
{
	private static readonly Dictionary<ImGuiMouseCursor, MouseCursor> mouseCursorMap = new()
	{
		{ ImGuiMouseCursor.Arrow, MouseCursor.MOUSE_CURSOR_ARROW },
		{ ImGuiMouseCursor.TextInput, MouseCursor.MOUSE_CURSOR_IBEAM },
		{ ImGuiMouseCursor.Hand, MouseCursor.MOUSE_CURSOR_POINTING_HAND },
		{ ImGuiMouseCursor.ResizeAll, MouseCursor.MOUSE_CURSOR_RESIZE_ALL },
		{ ImGuiMouseCursor.ResizeEW, MouseCursor.MOUSE_CURSOR_RESIZE_EW },
		{ ImGuiMouseCursor.ResizeNESW, MouseCursor.MOUSE_CURSOR_RESIZE_NESW },
		{ ImGuiMouseCursor.ResizeNS, MouseCursor.MOUSE_CURSOR_RESIZE_NS },
		{ ImGuiMouseCursor.ResizeNWSE, MouseCursor.MOUSE_CURSOR_RESIZE_NWSE },
		{ ImGuiMouseCursor.NotAllowed, MouseCursor.MOUSE_CURSOR_NOT_ALLOWED },
	};

	private static MouseCursor current = MouseCursor.MOUSE_CURSOR_DEFAULT;

	public static void Update(Gizmo hotControl, Gizmo hoveredControl)
	{
		// Integrate with ImGui (set IO.ConfigFlags to NoMouseCursorChange during Initialize).
		MouseCursor cursorType = mouseCursorMap[ImGui.GetMouseCursor()];

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
			Raylib.SetMouseCursor(cursor);
		}
	}
}