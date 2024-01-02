using Raylib_cs;

public class GizmoFactory
{
	private const float handleSize = 10f;
	private readonly float halfHandleSize = handleSize / 2f;
		
	public IEnumerable<Gizmo> Create(int id, Dictionary<int, Rect> items)
	{
		var gizmo = new Gizmo(id,
			() => items[id],
			delta =>
			{
				Rect r = items[id];
				r.X += delta.X;
				r.Y += delta.Y;
				items[id] = r;
			},
			hoverCursor: null,
			hotCursor: MouseCursor.MOUSE_CURSOR_RESIZE_ALL);
		yield return gizmo;

		var topRight = new Gizmo(null,
			() => new Rect(gizmo.Rect.X + gizmo.Rect.Width - halfHandleSize, gizmo.Rect.Y - halfHandleSize, handleSize, handleSize),
			delta =>
			{
				Rect newRect = gizmo.Rect;
				newRect.Width += delta.X;
				newRect.Y += delta.Y;
				newRect.Height -= delta.Y;
				items[(int)gizmo.UserData] = newRect;
			},
			MouseCursor.MOUSE_CURSOR_RESIZE_NESW);
		yield return topRight;
		gizmo.NextInGroup = topRight;
		
		var bottomLeft = new Gizmo(null,
			() => new Rect(gizmo.Rect.X - halfHandleSize, gizmo.Rect.Y + gizmo.Rect.Height - halfHandleSize, handleSize, handleSize),
			delta =>
			{
				Rect newRect = gizmo.Rect;
				newRect.X += delta.X;
				newRect.Width -= delta.X;
				newRect.Height += delta.Y;
				items[(int)gizmo.UserData] = newRect;
			},
			MouseCursor.MOUSE_CURSOR_RESIZE_NESW);
		yield return bottomLeft;
		topRight.NextInGroup = bottomLeft;
	}
}