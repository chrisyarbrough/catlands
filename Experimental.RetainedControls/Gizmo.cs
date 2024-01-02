using System.Numerics;
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
	public readonly MouseCursor? HoverCursor;
	public readonly MouseCursor? HotCursor;

	private readonly Action<Offset> set;
	private readonly Func<Rect> get;

	public Gizmo(object userData, Func<Rect> get, Action<Offset> set, MouseCursor? hoverCursor, MouseCursor? hotCursor = null)
	{
		UserData = userData;
		this.get = get;
		this.set = set;
		HoverCursor = hoverCursor;
		HotCursor = hotCursor ?? hoverCursor;
	}

	public void Apply(Offset offset)
	{
		set.Invoke(offset);
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