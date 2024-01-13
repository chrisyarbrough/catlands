public class GizmoFactory
{
	public IEnumerable<Gizmo> Create(int id, Dictionary<int, Rect> items)
	{
		var group = new Gizmo[9];

		int index = 0;
		foreach (Gizmo gizmo in CreateGroup(id, items))
		{
			group[index++] = gizmo;
			gizmo.Group = group;
			yield return gizmo;
		}
	}

	private IEnumerable<Gizmo> CreateGroup(int id, Dictionary<int, Rect> items)
	{
		// Main gizmo
		var gizmo = new Gizmo(userData: id,
			getRect: () => items[id],
			setRect: delta => items[id] = items[id].Translate(delta),
			parent: null);

		yield return gizmo;

		// Child handles
		foreach (var (getRect, applyDelta, isCorner) in CreateHandles(gizmo))
		{
			var handle = new Gizmo(userData: null,
				getRect,
				setRect: delta => items[id] = applyDelta.Invoke(delta, gizmo.Rect),
				parent: gizmo);

			handle.IsCorner = isCorner;
			yield return handle;
		}
	}

	private IEnumerable<(
			Func<Rect> getRect,
			Func<Coord, Rect, Rect> applyDelta,
			bool isCorner)>
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

		int HandleSize() => Math.Min(20, Math.Min(gizmo.Rect.Width / 2, gizmo.Rect.Height / 2));

		yield return (() => Rect.FromPointsV(new (gizmo.X0, gizmo.Y0), new (gizmo.X0, gizmo.Y1), HandleSize()), AdjustX0, false);
		yield return (() => Rect.FromPointsH(new (gizmo.X0, gizmo.Y0), new (gizmo.X1, gizmo.Y0), HandleSize()), AdjustY0, false);
		yield return (() => Rect.FromPointsH(new (gizmo.X0, gizmo.Y1), new (gizmo.X1, gizmo.Y1), HandleSize()), AdjustY1, false);
		yield return (() => Rect.FromPointsV(new (gizmo.X1, gizmo.Y0), new (gizmo.X1, gizmo.Y1), HandleSize()), AdjustX1, false);

		yield return (() => Rect.FromPoint(new (gizmo.X1, gizmo.Y0), HandleSize()), AdjustX1Y0, true);
		yield return (() => Rect.FromPoint(new (gizmo.X1, gizmo.Y1), HandleSize()), AdjustX1Y1, true);
		yield return (() => Rect.FromPoint(new (gizmo.X0, gizmo.Y1), HandleSize()), AdjustX0Y1, true);
		yield return (() => Rect.FromPoint(new (gizmo.X0, gizmo.Y0), HandleSize()), AdjustX0Y0, true);

		// @formatter:on
	}
}