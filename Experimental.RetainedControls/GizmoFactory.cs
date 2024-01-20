public class GizmoFactory
{
	public IEnumerable<Gizmo> Create(int id, Dictionary<int, Rect> rects)
	{
		var group = new Gizmo[9];

		int index = 0;
		foreach (Gizmo gizmo in CreateGroup(id, rects))
		{
			group[index++] = gizmo;
			gizmo.Group = group;
			yield return gizmo;
		}
	}

	private IEnumerable<Gizmo> CreateGroup(int id, Dictionary<int, Rect> rects)
	{
		// Main gizmo
		var gizmo = new Gizmo(
			userData: id,
			getRect: () => rects[id],
			setRect: delta => rects[id] = rects[id].Translate(delta));

		yield return gizmo;

		// Child handles
		foreach (var (points, getRect, handleSize, applyDelta, cornerIndex) in CreateHandles(gizmo))
		{
			var handle = new ResizeGizmo(
				points,
				handleSize,
				getRect: () => getRect.Invoke(points().Item1, points().Item2, handleSize()),
				setRect: delta => rects[id] = applyDelta.Invoke(delta, gizmo.Rect),
				parent: gizmo);

			handle.IsCorner = cornerIndex != -1;
			handle.CornerIndex = cornerIndex;
			yield return handle;
		}
	}

	private IEnumerable<(
			Func<(Coord, Coord)> points,
			Func<Coord, Coord, int, Rect> getRect,
			Func<int>,
			Func<Coord, Rect, Rect> applyDelta,
			int cornerIndex)>
		CreateHandles(Gizmo gizmo)
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

		int HandleSize() => Math.Min(20 * Gizmo.LineThickness, Math.Min(gizmo.Rect.Width / 2, gizmo.Rect.Height / 2));

		// Sides
		yield return (() => Shorten(new (gizmo.X0, gizmo.Y0), new (gizmo.X0, gizmo.Y1), HandleSize()), Rect.FromPointsV, HandleSize, AdjustX0, -1);
		yield return (() => Shorten(new (gizmo.X0, gizmo.Y0), new (gizmo.X1, gizmo.Y0), HandleSize()), Rect.FromPointsH, HandleSize, AdjustY0, -1);
		yield return (() => Shorten(new (gizmo.X0, gizmo.Y1), new (gizmo.X1, gizmo.Y1), HandleSize()), Rect.FromPointsH, HandleSize, AdjustY1, -1);
		yield return (() => Shorten(new (gizmo.X1, gizmo.Y0), new (gizmo.X1, gizmo.Y1), HandleSize()), Rect.FromPointsV, HandleSize, AdjustX1, -1);

		// Corners
		yield return (() => (new (gizmo.X1, gizmo.Y0), new (gizmo.X1, gizmo.Y0)), CornerFromPoint, HandleSize, AdjustX1Y0, 0);
		yield return (() => (new (gizmo.X1, gizmo.Y1), new (gizmo.X1, gizmo.Y1)), CornerFromPoint, HandleSize, AdjustX1Y1, 1);
		yield return (() => (new (gizmo.X0, gizmo.Y1), new (gizmo.X0, gizmo.Y1)), CornerFromPoint, HandleSize, AdjustX0Y1, 2);
		yield return (() => (new (gizmo.X0, gizmo.Y0), new (gizmo.X0, gizmo.Y0)), CornerFromPoint, HandleSize, AdjustX0Y0, 3);

		// @formatter:on
	}

	private (Coord, Coord) Shorten(Coord a, Coord b, int handleSize)
	{
		int handleExtents = handleSize / 2;
		Coord direction = (b - a).Normalize();
		return (a + direction * handleExtents, b - direction * handleExtents);
	}

	private static Rect CornerFromPoint(Coord a, Coord b, int handleSize)
	{
		return Rect.FromPoint(a, handleSize);
	}
}