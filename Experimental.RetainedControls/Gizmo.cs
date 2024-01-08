using System.Diagnostics;
using System.Numerics;
using Raylib_cs;

public class Gizmo
{
	public static Gizmo HotControl;
	public static Gizmo HoveredControl;
	public static readonly HashSet<Gizmo> Selection = new();

	private static int nextDebugId;
	private static readonly MouseCursors mouseCursor = new(new SectorGraph(8));

	public Gizmo Parent => parent;
	public Gizmo NextInGroup;

	public IEnumerable<Gizmo> AllInGroup()
	{
		Debug.Assert(Parent == null, "Groups can only be queried from a root gizmo.");
		Gizmo g = this;
		while (g != null)
		{
			yield return g;
			g = g.NextInGroup;
		}
	}

	public Rect Rect => get.Invoke();

	public bool IsSelected => Selection.Contains(this);

	public readonly object UserData;
	public readonly string DebugName;

	public override string ToString() => DebugName;

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
		this.DebugName = nextDebugId.ToString();
		nextDebugId++;
	}

	public void Apply(Coord delta)
	{
		set.Invoke(delta);
	}

	public MouseCursor GetMouseCursor()
	{
		if (parent != null)
		{
			Vector2 direction = Rect.Center - parent.Rect.Center;
			return mouseCursor.GetCursor(direction);
		}

		return MouseCursor.MOUSE_CURSOR_DEFAULT;
	}

	public void Draw()
	{
		Color color = GetColor();
		Raylib.DrawRectangleLinesEx((Rectangle)Rect, lineThick: 1f, color);
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