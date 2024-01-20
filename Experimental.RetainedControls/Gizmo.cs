using System.Numerics;
using Raylib_cs;

public class Gizmo
{
	public static Gizmo HotControl;
	public static Gizmo HoveredControl;
	public static readonly HashSet<Gizmo> Selection = new();
	public static bool DebugDraw = false;

	private static int nextDebugId;

	public IEnumerable<Gizmo> Group;
	public bool IsSelected => Selection.Contains(this);

	public bool IsCorner { get; set; }

	public virtual Rect Rect => getRect.Invoke();

	public int X0 => Rect.X0;
	public int Y0 => Rect.Y0;
	public int X1 => Rect.X1;
	public int Y1 => Rect.Y1;

	public readonly object UserData;
	public readonly string DebugName;

	public override string ToString() => DebugName;

	private readonly Action<Coord> setRect;
	private readonly Func<Rect> getRect;

	/// <summary>
	/// Carries over the fractional part of the drag movement because only the integer part is applied to the model.
	/// </summary>
	protected Vector2 FractionalDragOffset;

	protected Vector2 MouseDownOffset;

	public const int LineThickness = 15;

	public Gizmo(
		object userData,
		Func<Rect> getRect,
		Action<Coord> setRect)
	{
		UserData = userData;
		this.getRect = getRect;
		this.setRect = setRect;

		DebugName = nextDebugId.ToString();
		nextDebugId++;
	}

	public void Translate(Coord delta)
	{
		setRect.Invoke(delta);
	}

	public void SetPosition(Vector2 position)
	{
		Translate(new Coord(position - Rect.Center));
	}

	public virtual MouseCursor GetMouseCursor()
	{
		return MouseCursor.MOUSE_CURSOR_DEFAULT;
	}

	public virtual void Draw()
	{
		Color color = GetColor();
		//Raylib.DrawRectangleLinesEx((Rectangle)Rect, LineThickness, color);

		// Vector2[] points = Points().ToArray();
		// for (int i = 0; i < points.Length; i++)
		// {
		// 	Raylib.DrawLineV(points[i], points[(i + 1) % points.Length], Color.GREEN);
		// }
	}

	private const float handleSize = 20;

	protected Color GetColor()
	{
		if (HotControl == this)
			return Color.LIGHTGRAY;

		if (HoveredControl == this)
			return Color.YELLOW;

		if (Group.Any(x => HoveredControl == x))
			return Color.WHITE;

		if (Selection.Contains(this))
			return Color.ORANGE;

		return Color.LIGHTGRAY;
	}

	public virtual void OnMousePressed(Vector2 mousePosition)
	{
		FractionalDragOffset = Vector2.Zero;
		MouseDownOffset = mousePosition - Rect.Center;
	}

	public void Update(Vector2 mousePosition, List<Gizmo> gizmos)
	{
		if (HandleSnapping(mousePosition, gizmos))
			return;

		UpdateImpl(mousePosition, gizmos);
	}

	protected virtual void UpdateImpl(Vector2 mousePosition, List<Gizmo> _)
	{
		Vector2 delta = Raylib.GetMouseDelta() + FractionalDragOffset;
		FractionalDragOffset = new Vector2(delta.X - (int)delta.X, delta.Y - (int)delta.Y);
		Translate(new Coord(delta));
	}

	protected bool HandleSnapping(Vector2 mousePosition, List<Gizmo> gizmos)
	{
		if (Raylib.IsKeyDown(KeyboardKey.KEY_V))
		{
			// Snap to closest other gizmo handle.
			Vector2 closest = gizmos
				.Where(g => !Group.Contains(g))
				.Select(x => x.Rect.Center).MinBy(x => Vector2.DistanceSquared(x, mousePosition));

			Raylib.DrawCircleV(closest, 7, Color.WHITE);
			SetPosition(closest);
			return true;
		}

		if (Raylib.IsKeyReleased(KeyboardKey.KEY_V))
		{
			ResetToMouse(mousePosition);
			return true;
		}

		return false;
	}

	protected void ResetToMouse(Vector2 mousePosition)
	{
		Translate(delta: new Coord(mousePosition - MouseDownOffset - Rect.Center));
	}
}