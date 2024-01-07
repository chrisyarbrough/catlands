public class GizmoFactory
{
	private const int handleSize = 50;

	public IEnumerable<Gizmo> Create(int id, Dictionary<int, Rect> items)
	{
		var gizmo = new Gizmo(userData: id,
			get: () => items[id],
			set: delta =>
			{
				Rect r = items[id];
				r.Translate(delta);
				items[id] = r;
			},
			parent: null);

		yield return gizmo;

		Gizmo previousGizmo = gizmo;

		foreach (var (center, applyDelta, size) in GetHandles(gizmo))
		{
			var handle = new Gizmo(userData: null,
				get: () => Rect.Handle(center(), size()),
				set: delta =>
				{
					Rect r = gizmo.Rect;
					r = applyDelta.Invoke(delta, r);
					items[id] = r;
				},
				parent: gizmo);

			yield return handle;

			previousGizmo.NextInGroup = handle;
			previousGizmo = handle;
		}
	}

	private IEnumerable<(
			Func<Coord> center,
			Func<Coord, Rect, Rect> applyDelta,
			Func<(int, int)> size)>
		GetHandles(Gizmo gizmo)
	{
		/*
		 * x0,y0 xM,y0 x1,y0
		 * x0,yM       x1,yM
		 * x0,y1 xM,y1 x1,y1
		 */
		
		// @formatter:off
		
		// Single-axis
		Rect AdjustX0(Coord delta, Rect rect) { rect.X0 += delta.X; return rect; }
		Rect AdjustY0(Coord delta, Rect rect) { rect.Y0 += delta.Y; return rect; }
		Rect AdjustX1(Coord delta, Rect rect) { rect.X1 += delta.X; return rect; }
		Rect AdjustY1(Coord delta, Rect rect) { rect.Y1 += delta.Y; return rect; }

		// Two-axis (corners)
		Rect AdjustX0Y0(Coord delta, Rect rect) => AdjustX0(delta, AdjustY0(delta, rect));
		Rect AdjustX1Y0(Coord delta, Rect rect) => AdjustX1(delta, AdjustY0(delta, rect));
		Rect AdjustX0Y1(Coord delta, Rect rect) => AdjustX0(delta, AdjustY1(delta, rect));
		Rect AdjustX1Y1(Coord delta, Rect rect) => AdjustX1(delta, AdjustY1(delta, rect));
		
		int SmallestSize() => Math.Min(handleSize, Math.Min(gizmo.Rect.Width, gizmo.Rect.Height));
		Func<(int, int)> handleSizeCorner =     () => (SmallestSize(),    SmallestSize());
		Func<(int, int)> handleSizeHorizontal = () => (Math.Max(0, gizmo.Rect.Width - SmallestSize()), SmallestSize());
		Func<(int, int)> handleSizeVertical =   () => (SmallestSize(),     Math.Max(0, gizmo.Rect.Height - SmallestSize()));

		yield return (() => new Coord(gizmo.Rect.X0,     gizmo.Rect.YMid()), AdjustX0, handleSizeVertical);
		yield return (() => new Coord(gizmo.Rect.XMid(), gizmo.Rect.Y0),     AdjustY0, handleSizeHorizontal);
		yield return (() => new Coord(gizmo.Rect.X1,     gizmo.Rect.YMid()), AdjustX1, handleSizeVertical);
		yield return (() => new Coord(gizmo.Rect.XMid(), gizmo.Rect.Y1),     AdjustY1, handleSizeHorizontal);
		
		yield return (() => new Coord(gizmo.Rect.X0,     gizmo.Rect.Y0),     AdjustX0Y0, handleSizeCorner);
		yield return (() => new Coord(gizmo.Rect.X1,     gizmo.Rect.Y0),     AdjustX1Y0, handleSizeCorner);
		yield return (() => new Coord(gizmo.Rect.X0,     gizmo.Rect.Y1),     AdjustX0Y1, handleSizeCorner);
		yield return (() => new Coord(gizmo.Rect.X1,     gizmo.Rect.Y1),     AdjustX1Y1, handleSizeCorner);

		// @formatter:on
	}
}

public static class RectExtensions
{
	public static int XMid(this Rect rect) => (rect.X0 + rect.X1) / 2;
	public static int YMid(this Rect rect) => (rect.Y0 + rect.Y1) / 2;
}