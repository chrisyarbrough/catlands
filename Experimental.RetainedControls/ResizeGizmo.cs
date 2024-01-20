using System.Numerics;
using Raylib_cs;

public class ResizeGizmo : Gizmo
{
	private readonly Func<(Coord, Coord)> points;
	private readonly Func<int> handleSize;
	public Gizmo Parent { get; }

	public Vector2 OppositeCorner => 2 * Parent.Rect.Center - Rect.Center;

	public override Rect Rect
	{
		get
		{
			Vector2 s = points.Invoke().Item1;
			Vector2 t = points.Invoke().Item2;

			if (IsCorner)
			{
				return Rect.Handle(s, handleSize());
			}
			else
			{
				if (Math.Abs(s.X - t.X) > Math.Abs(s.Y - t.Y))
				{
					// Horizontal
					int extents = handleSize() / 2;
					return new Rect(s.X, s.Y - extents, t.X - s.X, handleSize());
				}
				else
				{
					// Vertical
					int extents = handleSize() / 2;
					return new Rect(s.X - extents, s.Y, handleSize(), t.Y - s.Y);
				}
			}
		}
	}

	public Gizmo OppositeGizmo => Parent.Group.MinBy(x => Vector2.Distance(x.Rect.Center, OppositeCorner));
	public int CornerIndex { get; set; }

	private static readonly MouseCursors mouseCursor = new(new SectorGraph(8));

	private Vector2 originalOppositeControl;
	private DottedLine dottedLine;

	public ResizeGizmo(
		Func<(Coord, Coord)> points, Func<int> handleSize,
		Func<Rect> getRect, Action<Coord> setRect, Gizmo parent)
		: base(userData: null, getRect, setRect)
	{
		this.points = points;
		this.handleSize = handleSize;
		Parent = parent;
	}

	public override MouseCursor GetMouseCursor()
	{
		Rectangle r = (Rectangle)Parent.Rect;

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
			Parent.Rect.Center, rightMid, bottomRight, bottomMid, bottomLeft, leftMid, topLeft, topMid, topRight);

		return mouseCursor.GetCursor(Rect.Center);
	}

	public override void OnMousePressed(Vector2 mousePosition)
	{
		base.OnMousePressed(mousePosition);
		originalOppositeControl = OppositeGizmo.Rect.Center;
		dottedLine = new DottedLine(Rect.Center, OppositeCorner);
	}

	private readonly TriLine line = new(LineThickness, pointCount: 3);

	public override void Draw()
	{
		if (DebugDraw)
		{
			base.Draw();
			Raylib.DrawPixel((int)Rect.Center.X, (int)Rect.Center.Y, Color.RED);
		}
		else
		{
			Vector2 s = points.Invoke().Item1;
			Vector2 t = points.Invoke().Item2;

			if (IsCorner)
			{
				//Raylib.DrawRectangleLinesEx((Rectangle)Rect, LineThickness, Color.GREEN);

				// @formatter:off

				// Top Right
				//float y = (Rect.Y0 + Rect.Y1) / 2f;
				float y= s.Y;
				float x= s.X;
				Vector2 left =   new(Rect.X0, y);
				Vector2 right =  new(x + LineThickness / 2f, y);
				Vector2 top =    new(x, y);
				Vector2 bottom = new(x, Rect.Y1);

				// @formatter:on

				Vector2 c = s + (t - s) / 2f;

				double angle = FindSector(Parent.Rect.Center, c);

				Vector2 rotatedLeft = RotatePoint(c, left, angle);
				Vector2 rotatedRight = RotatePoint(c, right, angle);
				Vector2 rotatedTop = RotatePoint(c, top, angle);
				Vector2 rotatedBottom = RotatePoint(c, bottom, angle);
				//
				// line.UpdatePoint(0, rotatedLeft);
				// line.UpdatePoint(1, rotatedMiddle);
				// line.UpdatePoint(2, rotatedBottom);
				// line.Triangulate();
				// line.Draw(GetColor());
				Raylib.DrawLineEx(rotatedLeft, rotatedRight, LineThickness, GetColor());
				Raylib.DrawLineEx(rotatedTop, rotatedBottom, LineThickness, GetColor());

				Raylib.DrawLineV(rotatedLeft, rotatedTop, Color.RED);
				Raylib.DrawLineV(rotatedTop, rotatedBottom, Color.RED);
			}
			else
			{
				//Raylib.DrawRectangleLinesEx((Rectangle)Rect, LineThickness, Color.GREEN);
				Raylib.DrawLineEx(s, t, LineThickness, GetColor());
				Raylib.DrawLineV(s, t, Color.RED);
			}

			Raylib.DrawPixel(Parent.Rect.X0, Parent.Rect.Y0, Color.MAGENTA);
			Raylib.DrawPixel(Parent.Rect.X1, Parent.Rect.Y1, Color.MAGENTA);
			Raylib.DrawPixel(Parent.Rect.X0, Parent.Rect.Y1, Color.MAGENTA);
			Raylib.DrawPixel(Parent.Rect.X1, Parent.Rect.Y0, Color.MAGENTA);
		}
	}

	public double FindSector(Vector2 center, Vector2 point)
	{
		float deltaY = point.Y - center.Y;
		float deltaX = point.X - center.X;
		double angle = Math.Atan2(deltaY, deltaX);
		double angleNormalized = (angle + 2 * Math.PI) % (2 * Math.PI);
		return Math.PI / 2 * Math.Ceiling(angleNormalized / (Math.PI / 2));
	}

	public Vector2 RotatePoint(Vector2 center, Vector2 point, double angle)
	{
		// [cos(theta), -sin(theta)]
		// [sin(theta),  cos(theta)]
		double cosTheta = Math.Cos(angle);
		double sinTheta = Math.Sin(angle);

		double dx = point.X - center.X;
		double dy = point.Y - center.Y;

		double newX = cosTheta * dx - sinTheta * dy + center.X;
		double newY = sinTheta * dx + cosTheta * dy + center.Y;

		return new Vector2((float)newX, (float)newY);
	}

	protected override void UpdateImpl(Vector2 mousePosition, List<Gizmo> gizmos)
	{
		if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT) && IsCorner)
		{
			Vector2 snappedPosition = dottedLine.ClosestPointTo(Raylib.GetMousePosition() - MouseDownOffset);
			Raylib.DrawCircleV(snappedPosition, 5, Color.YELLOW);
			SetPosition(snappedPosition);
			dottedLine.Draw(Rect.Center, OppositeCorner);

			if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_ALT))
			{
				Vector2 oppositePosition = 2 * dottedLine.Center - snappedPosition;
				oppositePosition = dottedLine.ClosestPointTo(oppositePosition);
				OppositeGizmo.SetPosition(oppositePosition);
				Raylib.DrawCircleV(oppositePosition, 5, Color.SKYBLUE);
			}
		}
		else
		{
			base.UpdateImpl(mousePosition, gizmos);

			if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_ALT))
			{
				Vector2 oppositePosition = 2 * dottedLine.Center - Rect.Center;
				OppositeGizmo.SetPosition(oppositePosition);
			}
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