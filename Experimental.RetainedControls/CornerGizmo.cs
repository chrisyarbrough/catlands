using System.Numerics;
using Raylib_cs;

public class CornerGizmo : Gizmo
{
	public override Rect Rect => Rect.Handle(PointA.Invoke(), HandleSize());

	public Gizmo Parent { get; }

	protected Vector2 OppositePoint => 2 * Parent.Rect.Center - Rect.Center;

	protected Gizmo OppositeGizmo => Parent.Group.MinBy(x => Vector2.Distance(x.Rect.Center, OppositePoint));
	protected int LineWidth => Math.Min(15, Math.Min(Parent.Rect.Width / 2, Parent.Rect.Height / 2));

	protected readonly Func<Coord> PointA;
	protected readonly Func<int> HandleSize;

	private DottedLine dottedLine;

	private static readonly MouseCursors mouseCursor = new(new SectorGraph(8));

	public CornerGizmo(
		Func<Coord> point,
		Action<Coord> setRect,
		Gizmo parent) : base(userData: null, getRect: null, setRect)
	{
		PointA = point;
		Parent = parent ?? throw new ArgumentNullException(nameof(parent));
		HandleSize = () => (int)Math.Min(80f, Math.Min(Parent.Rect.Width / 2f, Parent.Rect.Height / 2f));
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
		dottedLine = new DottedLine(Rect.Center, OppositePoint);
	}

	protected override void DrawImpl()
	{
		// Draw two perpendicular lines with a little bit of overlap to make the top right corner.
		// Then rotate around the parent center for the other corners.
		Vector2 corner = PointA.Invoke();
		float overlap = LineWidth / 2f;
		double angle = SectorAngle(Parent.Rect.Center, corner);

		Vector2 left = RotatePoint(corner, new(Rect.X0, corner.Y), angle);
		Vector2 right = RotatePoint(corner, new(corner.X + overlap, corner.Y), angle);

		Vector2 top = RotatePoint(corner, new(corner.X, corner.Y - overlap), angle);
		Vector2 bottom = RotatePoint(corner, new(corner.X, Rect.Y1), angle);

		Raylib.DrawLineEx(left, right, LineWidth, GetColor());
		Raylib.DrawLineEx(top, bottom, LineWidth, GetColor());
	}

	protected override Color DebugColor => Color.GREEN;
	
	protected Color GetColor()
	{
		if (HotControl == this)
			return Color.YELLOW;

		if (HoveredControl == this)
			return Color.GOLD;

		if (HoveredControl == Parent)
			return Color.GOLD;

		if (Selection.Contains(Parent))
			return Color.ORANGE;

		return Color.LIGHTGRAY;
	}

	protected override void UpdateImpl(Vector2 mousePosition)
	{
		if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
		{
			Vector2 snappedPosition = dottedLine.ClosestPointTo(Raylib.GetMousePosition() - MouseDownOffset);
			Raylib.DrawCircleV(snappedPosition, 5, Color.YELLOW);
			SetPosition(snappedPosition);
			dottedLine.Draw(Rect.Center, OppositePoint);

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
			base.UpdateImpl(mousePosition);
		}
	}

	private double SectorAngle(Vector2 center, Vector2 point)
	{
		float deltaY = point.Y - center.Y;
		float deltaX = point.X - center.X;
		double angle = Math.Atan2(deltaY, deltaX);
		double angleNormalized = (angle + 2 * Math.PI) % (2 * Math.PI);
		return Math.PI / 2 * Math.Ceiling(angleNormalized / (Math.PI / 2));
	}

	private Vector2 RotatePoint(Vector2 center, Vector2 point, double angle)
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
}