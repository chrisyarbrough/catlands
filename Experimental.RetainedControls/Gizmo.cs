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
	public IEnumerable<Gizmo> Group;
	public bool IsSelected => Selection.Contains(this);
	
	public bool IsCorner { get; set; }

	public Rect Rect => getRect.Invoke();

	public int X0 => Rect.X0;
	public int Y0 => Rect.Y0;
	public int X1 => Rect.X1;
	public int Y1 => Rect.Y1;

	public Vector2 OppositeCorner
	{
		get
		{
			Debug.Assert(parent != null);
			return 2 * parent.Rect.Center - Rect.Center;
		}
	}

	public Gizmo OppositeGizmo
	{
		get
		{
			Debug.Assert(parent != null);
			return parent.Group.MinBy(x => Vector2.Distance(x.Rect.Center, OppositeCorner));
		}
	}

	public readonly object UserData;
	public readonly string DebugName;

	public override string ToString() => DebugName;

	private readonly Action<Coord> setRect;
	private readonly Func<Rect> getRect;
	private readonly Gizmo parent;

	public Gizmo(
		object userData,
		Func<Rect> getRect,
		Action<Coord> setRect,
		Gizmo parent)
	{
		UserData = userData;
		this.getRect = getRect;
		this.setRect = setRect;
		this.parent = parent;
		this.DebugName = nextDebugId.ToString();
		nextDebugId++;
	}

	public void Apply(Coord delta)
	{
		setRect.Invoke(delta);
	}

	public void SetPosition(Vector2 position)
	{
		Apply(new Coord(position - Rect.Center));
	}

	public MouseCursor GetMouseCursor()
	{
		if (parent != null)
		{
			Rectangle r = (Rectangle)parent.Rect;

			// @formatter:off
			Vector2 rightMid =    new(r.X + r.Width,      r.Y + r.Height / 2f);
			Vector2 bottomRight = new(r.X + r.Width,      r.Y + r.Height);
			Vector2 bottomMid =   new(r.X + r.Width / 2f, r.Y + r.Height);
			Vector2 bottomLeft =  new(r.X,                r.Y + r.Height);
			Vector2 leftMid =     new(r.X,                r.Y + r.Height / 2f);
			Vector2 topLeft =     new(r.X,                r.Y);
			Vector2 topMid =      new(r.X + r.Width / 2f, r.Y);
			Vector2 topRight =    new(r.X + r.Width,      r.Y);
			// @formatter:on

			mouseCursor.UpdateDirectionsFromPoints(
				parent.Rect.Center, rightMid, bottomRight, bottomMid, bottomLeft, leftMid, topLeft, topMid, topRight);

			return mouseCursor.GetCursor(Rect.Center);
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
			return Color.LIGHTGRAY;

		if (HoveredControl == this)
			return Color.YELLOW;

		if (Parent == null && Group.Any(x => HoveredControl == x))
			return Color.WHITE;

		if (Selection.Contains(this))
			return Color.ORANGE;

		return Color.LIGHTGRAY;
	}
}