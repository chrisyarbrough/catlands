using System.Numerics;
using Raylib_cs;

public static class Handles
{
	public static int? HoveredControl { get; private set; }
	public static int? HotControl { get; set; }

	private static int nextControlId;

	private static int? closestId;
	private static float smallestArea = float.MaxValue;
	private static float smallestDistance = float.MaxValue;
	private static Event current;
	
	public static Vector2 fractionalOffset;

	public static void BeginLayoutPhase(Event current)
	{
		Handles.current = current;
		nextControlId = 1;
		closestId = null;
		smallestArea = float.MaxValue;
		smallestDistance = float.MaxValue;
		HoveredControl = null;
	}

	public static void BeginDrawPhase(Event current)
	{
		Handles.current = current;
		nextControlId = 1;

		if (HotControl == null)
			HoveredControl = closestId;

		if (current.IsMousePressed)
			HotControl = closestId;
	}

	public static int GetControlId()
	{
		return nextControlId++;
	}

	public static Vector2 FreeMove(Vector2 center, Vector2 size)
	{
		int controlId = GetControlId();
		return FreeMove(controlId, center, size);
	}

	public static Vector2 FreeMove(int controlId, Vector2 center, Vector2 size, bool isSelected = false)
	{
		Rectangle rect = new(center.X - size.X / 2f, center.Y - size.Y / 2f, size.X, size.Y);

		if (current.Phase == EventPhase.Draw)
		{
			Color color = GetColor(controlId, isSelected);
			Raylib.DrawRectangleLinesEx(rect, 1f, color);

			if (controlId == HotControl)
			{
				if (current.IsMouseReleased)
					HotControl = null;

				Vector2 delta = Raylib.GetMouseDelta() + fractionalOffset;
				fractionalOffset = new Vector2(delta.X - (int)delta.X, delta.Y - (int)delta.Y);
				return delta;
			}
		}
		else if (Raylib.CheckCollisionPointRec(current.MousePos, rect))
		{
			float distance = Vector2.Distance(current.MousePos, center);

			float area = rect.Width * rect.Height;
			if (distance < smallestDistance || area < smallestArea)
			{
				smallestArea = area;
				smallestDistance = distance;
				closestId = controlId;
			}
		}

		return Vector2.Zero;
	}

	private static Color GetColor(int id, bool isSelected)
	{
		if (HotControl == id)
			return Color.RED;

		if (HoveredControl == id)
			return Color.YELLOW;

		if (isSelected)
			return Color.ORANGE;

		return Color.LIGHTGRAY;
	}
}