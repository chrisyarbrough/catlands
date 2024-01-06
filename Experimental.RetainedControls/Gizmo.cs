using Raylib_cs;

public class Gizmo
{
	public static Gizmo HotControl;
	public static Gizmo HoveredControl;
	public static readonly HashSet<Gizmo> Selection = new();

	public Gizmo NextInGroup;

	public IEnumerable<Gizmo> AllInGroup()
	{
		Gizmo g = this;
		while (g != null)
		{
			yield return g;
			g = g.NextInGroup;
		}
	}

	public Rect Rect => get.Invoke();

	public readonly object UserData;

	private readonly Action<Coord> set;
	private readonly Func<Rect> get;
	private readonly Gizmo parent;

	public Gizmo(
		object userData,
		Func<Rect> get,
		Action<Coord> set,
		Gizmo parent)
	{
		UserData = userData;
		this.get = get;
		this.set = set;
		this.parent = parent;
	}

	public void Apply(Coord delta)
	{
		set.Invoke(delta);
	}

	public void Draw()
	{
		Color color = GetColor();
		Raylib.DrawRectangleLinesEx((Rectangle)Rect, lineThick: 1f, color);
	}

	public MouseCursor GetMouseCursor()
	{
		if (parent != null)
		{
			Coord corner = Rect.Center;
			Coord center = parent.Rect.Center;

			// Two-axis (corners)
			if (corner.X < center.X && corner.Y < center.Y)
				return MouseCursor.MOUSE_CURSOR_RESIZE_NWSE;
			else if (corner.X > center.X && corner.Y < center.Y)
				return MouseCursor.MOUSE_CURSOR_RESIZE_NESW;
			else if (corner.X > center.X && corner.Y > center.Y)
				return MouseCursor.MOUSE_CURSOR_RESIZE_NWSE;
			else if (corner.X < center.X && corner.Y > center.Y)
				return MouseCursor.MOUSE_CURSOR_RESIZE_NESW;

			// Single-axis
			if (corner.X < center.X)
				return MouseCursor.MOUSE_CURSOR_RESIZE_EW;
			else if (corner.X > center.X)
				return MouseCursor.MOUSE_CURSOR_RESIZE_EW;
			else if (corner.Y < center.Y)
				return MouseCursor.MOUSE_CURSOR_RESIZE_NS;
			else if (corner.Y > center.Y)
				return MouseCursor.MOUSE_CURSOR_RESIZE_NS;
		}

		return MouseCursor.MOUSE_CURSOR_DEFAULT;
	}

	private Color GetColor()
	{
		if (HotControl == this)
			return Color.RED;

		if (HoveredControl == this)
			return Color.YELLOW;

		if (Selection.Contains(this))
			return Color.ORANGE;

		return Color.LIGHTGRAY;
	}
}