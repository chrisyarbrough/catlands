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
		var mainGizmo = new Gizmo(
			userData: id,
			getRect: () => rects[id],
			setRect: delta => rects[id] = rects[id].Translate(delta));

		yield return mainGizmo;

		foreach (Gizmo handle in CreateHandles(mainGizmo, rect => rects[id] = rect))
		{
			yield return handle;
		}
	}

	private IEnumerable<Gizmo> CreateHandles(Gizmo gizmo, Action<Rect> setter)
	{
		// @formatter:off

		/*
		 * x0,y0 xM,y0 x1,y0
		 * x0,yM       x1,yM
		 * x0,y1 xM,y1 x1,y1
		 */
		
		// Single-axis
		Rect MoveX0(Coord delta, Rect rect) { rect.X0 += delta.X; return rect; }
		Rect MoveY0(Coord delta, Rect rect) { rect.Y0 += delta.Y; return rect; }
		Rect MoveX1(Coord delta, Rect rect) { rect.X1 += delta.X; return rect; }
		Rect MoveY1(Coord delta, Rect rect) { rect.Y1 += delta.Y; return rect; }

		// Two-axis (corners)
		Rect MoveX0Y0(Coord delta, Rect rect) => MoveX0(delta, MoveY0(delta, rect));
		Rect MoveX1Y0(Coord delta, Rect rect) => MoveX1(delta, MoveY0(delta, rect));
		Rect MoveX0Y1(Coord delta, Rect rect) => MoveX0(delta, MoveY1(delta, rect));
		Rect MoveX1Y1(Coord delta, Rect rect) => MoveX1(delta, MoveY1(delta, rect));

		int HandleSize() => (int)Math.Min(80f, Math.Min(gizmo.Rect.Width / 2f, gizmo.Rect.Height / 2f));
		
		// Sides
		yield return new SideGizmo(() => (new (gizmo.X0, gizmo.Y0), new (gizmo.X0, gizmo.Y1)), delta => setter.Invoke(MoveX0(delta, gizmo.Rect)), gizmo);
		yield return new SideGizmo(() => (new (gizmo.X0, gizmo.Y0), new (gizmo.X1, gizmo.Y0)), delta => setter.Invoke(MoveY0(delta, gizmo.Rect)), gizmo);
		yield return new SideGizmo(() => (new (gizmo.X0, gizmo.Y1), new (gizmo.X1, gizmo.Y1)), delta => setter.Invoke(MoveY1(delta, gizmo.Rect)), gizmo);
		yield return new SideGizmo(() => (new (gizmo.X1, gizmo.Y0), new (gizmo.X1, gizmo.Y1)), delta => setter.Invoke(MoveX1(delta, gizmo.Rect)), gizmo);
		
		// Corners
		yield return new CornerGizmo(() => new (gizmo.X1, gizmo.Y0), delta => setter.Invoke(MoveX1Y0(delta, gizmo.Rect)), gizmo);
		yield return new CornerGizmo(() => new (gizmo.X1, gizmo.Y1), delta => setter.Invoke(MoveX1Y1(delta, gizmo.Rect)), gizmo);
		yield return new CornerGizmo(() => new (gizmo.X0, gizmo.Y1), delta => setter.Invoke(MoveX0Y1(delta, gizmo.Rect)), gizmo);
		yield return new CornerGizmo(() => new (gizmo.X0, gizmo.Y0), delta => setter.Invoke(MoveX0Y0(delta, gizmo.Rect)), gizmo);

		// @formatter:on
	}

	
}