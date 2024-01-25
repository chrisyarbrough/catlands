using System.Numerics;
using Raylib_cs;

public class SideGizmo : CornerGizmo
{
	public override Rect Rect
	{
		get
		{
			(Vector2 s, Vector2 t) = Shorten(PointA.Invoke(), PointB.Invoke(), HandleSize());
			int extents = HandleSize() / 2;

			if (Math.Abs(s.X - t.X) > Math.Abs(s.Y - t.Y))
			{
				// Horizontal
				return new Rect(s.X, s.Y - extents, t.X - s.X, HandleSize());
			}
			else
			{
				// Vertical
				return new Rect(s.X - extents, s.Y, HandleSize(), t.Y - s.Y);
			}
		}
	}

	protected readonly Func<Coord> PointB;

	private Vector2 originalOppositeControl;
	private Vector2 originalCenter;

	public SideGizmo(
		Func<(Coord, Coord)> points,
		Action<Coord> setRect,
		Gizmo parent) : base(() => points.Invoke().Item1, setRect, parent)
	{
		PointB = () => points.Invoke().Item2;
	}
	
	private (Vector2, Vector2) Shorten(Coord a, Coord b, int handleSize)
	{
		int handleExtents = handleSize / 2;
		Coord direction = (b - a).Normalize();
		return (a + direction * handleExtents, b - direction * handleExtents);
	}

	public override void OnMousePressed(Vector2 mousePosition)
	{
		base.OnMousePressed(mousePosition);
		originalOppositeControl = OppositeGizmo.Rect.Center;
		originalCenter = (Rect.Center + OppositePoint) / 2f;
	}

	protected override void DrawImpl()
	{
		(Vector2 s, Vector2 t) = Shorten(PointA.Invoke(), PointB.Invoke(), HandleSize());

		Raylib.DrawLineEx(s, t, LineWidth, GetColor());

		if (HoveredControl == this)
		{
			Vector2 offset = Vector2.Normalize(Parent.Rect.Center - Rect.Center) * LineWidth;
			s += offset;
			t += offset;
			Raylib.DrawLineEx(s, t, LineWidth, GetColor() with { A = 40 });
		}
	}

	protected override Color DebugColor => Color.DARKGREEN;

	protected override void UpdateImpl(Vector2 mousePosition)
	{
		base.UpdateImpl(mousePosition);

		if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_ALT))
		{
			Vector2 oppositePosition = 2 * originalCenter - Rect.Center;
			OppositeGizmo.SetPosition(oppositePosition);
		}

		if (Raylib.IsKeyReleased(KeyboardKey.KEY_LEFT_ALT))
		{
			OppositeGizmo.SetPosition(originalOppositeControl);
		}

		if (Raylib.IsKeyReleased(KeyboardKey.KEY_LEFT_SHIFT))
		{
			ResetToMouse(mousePosition);
		}
	}
}