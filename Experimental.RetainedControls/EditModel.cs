using System.Numerics;
using Raylib_cs;

public class EditModel : EditModelBase
{
	private readonly List<Gizmo> gizmos = new();
	private readonly GizmoFactory gizmoFactory = new();

	public EditModel(Model model) : base(model)
	{
		RebuildGizmos();
	}

	public void RebuildGizmos()
	{
		gizmos.Clear();
		gizmos.AddRange(Rects.Keys.SelectMany(id => gizmoFactory.Create(id, Rects)));
	}

	public override int AddRect(Rect rect)
	{
		int id = base.AddRect(rect);
		gizmos.AddRange(gizmoFactory.Create(id, Rects));
		return id;
	}

	public void Update()
	{
		UpdateActiveGizmos(Raylib.GetMousePosition());
		gizmos.ForEach(gizmo => gizmo.Draw());
		HandleDebugDraw();
	}

	private void UpdateActiveGizmos(Vector2 mousePosition)
	{
		if (Gizmo.HotControl == null)
		{
			Gizmo.HoveredControl = SelectionStrategy.FindHoveredControl(mousePosition, gizmos);
			
			if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
			{
				OnMousePressed(mousePosition);
			}
		}
		else
		{
			Gizmo.HotControl.Update(mousePosition, gizmos);

			if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
			{
				Gizmo.HotControl = null;
				EvaluateChanged();
			}
		}
		
		Cursor.Update(Gizmo.HotControl, Gizmo.HoveredControl);
	}

	private void OnMousePressed(Vector2 mousePosition)
	{
		Gizmo.Selection.Clear();

		if (Gizmo.HoveredControl != null)
		{
			Gizmo.HotControl = Gizmo.HoveredControl;
			RecordUndo();

			if (Gizmo.HotControl.UserData != null)
			{
				Gizmo.Selection.Add(Gizmo.HotControl);
			}

			Gizmo.HotControl.OnMousePressed(mousePosition);
		}
	}

	private static void HandleDebugDraw()
	{
		if (Gizmo.DebugDraw)
		{
			// Draw hovered and hot controls on top of other controls.
			Gizmo.HoveredControl?.Draw();
			Gizmo.HotControl?.Draw();
		}
	}

	protected override void DeleteSelectedRects()
	{
		foreach (Gizmo selected in Gizmo.Selection)
		{
			Rects.Remove((int)selected.UserData);

			foreach (Gizmo gizmo in selected.Group)
				gizmos.Remove(gizmo);
		}

		Gizmo.HotControl = null;
		Gizmo.Selection.Clear();
	}

	public override void Undo()
	{
		base.Undo();
		RebuildGizmos();
	}
}