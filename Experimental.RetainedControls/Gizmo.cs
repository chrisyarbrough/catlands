using System.Numerics;
using Raylib_cs;

public class Gizmo
{
	public static Gizmo HotControl;
	public static Gizmo HoveredControl;
	public static readonly HashSet<Gizmo> Selection = new();

	public Gizmo Friend;

	public IEnumerable<Gizmo> Group()
	{
		Gizmo g = this;
		while (g != null)
		{
			yield return g;
			g = g.Friend;
		}
	}
	
	public Rectangle Rect => get.Invoke();

	public readonly object UserData;
	private readonly Action<Vector2> set;
	private readonly Func<Rectangle> get;

	public Gizmo(object userData, Func<Rectangle> get, Action<Vector2> set)
	{
		UserData = userData;
		this.get = get;
		this.set = set;
	}
	
	public void Move(Vector2 delta)
	{
		set.Invoke(delta);
	}

	public void Draw()
	{
		Color color = GetColor();
		Raylib.DrawRectangleLinesEx(get.Invoke(), 1f, color);
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