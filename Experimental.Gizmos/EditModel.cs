namespace Experimental.Gizmos;

using System.Numerics;
using ImGuiNET;
using Raylib_cs;

public class EditModel : EditModelBase
{
	private readonly List<Gizmo> gizmos = new();

	public EditModel(Model model) : base(model)
	{
		RebuildGizmos();
	}

	public void RebuildGizmos()
	{
		gizmos.Clear();
		gizmos.AddRange(Rects.Keys.SelectMany(id => GizmoFactory.Create(id, Rects)));
	}

	public override int AddRect(Rect rect)
	{
		int id = base.AddRect(rect);
		gizmos.AddRange(GizmoFactory.Create(id, Rects));
		return id;
	}

	public void Update(bool captureInput)
	{
		Vector2 mousePosition = Raylib.GetMousePosition();

		if (Gizmo.HotControl == null)
		{
			Gizmo.HoveredControl = SelectionStrategy.FindHoveredControl(mousePosition, gizmos);

			if (captureInput && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
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

		if (!captureInput)
		{
			Gizmo.HoveredControl = null;
		}

		ImGui.Checkbox("Debug Draw", ref Gizmo.DebugDraw);

		Cursor.Update(Gizmo.HotControl, Gizmo.HoveredControl);

		gizmos.ForEach(gizmo => gizmo.Draw());

		// Draw hovered and hot controls on top of other controls.
		Gizmo.HoveredControl?.Draw();
		Gizmo.HotControl?.Draw();
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