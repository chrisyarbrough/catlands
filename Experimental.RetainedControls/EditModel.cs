using System.Numerics;
using Raylib_cs;

public class EditModel : EditModelBase
{
	private readonly List<Gizmo> gizmos = new();
	private readonly GizmoFactory gizmoFactory = new();

	private static readonly bool debugDrawHandles = false;

	/// <summary>
	/// Carries over the fractional part of the drag movement because only the integer part is applied to the model.
	/// </summary>
	private Vector2 fractionalDragOffset;

	private Vector2 mouseDownOffset;

	public EditModel(Model model) : base(model)
	{
		RebuildGizmos();
	}

	public void RebuildGizmos()
	{
		gizmos.Clear();
		foreach (int id in model.Items.Keys)
		{
			AddGizmosForItem(id);
		}
	}

	private void AddGizmosForItem(int id)
	{
		foreach (Gizmo gizmo in gizmoFactory.Create(id, model.Items))
			gizmos.Add(gizmo);
	}

	public override int AddItem(Rect rect)
	{
		int id = base.AddItem(rect);
		AddGizmosForItem(id);
		return id;
	}

	public void Update()
	{
		Vector2 mousePosition = Raylib.GetMousePosition();

		if (Gizmo.HotControl == null)
		{
			Gizmo.HoveredControl = SelectionStrategy.FindHoveredControl(mousePosition, gizmos);
		}

		if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
		{
			Gizmo.Selection.Clear();
			fractionalDragOffset = Vector2.Zero;

			if (Gizmo.HoveredControl != null && Gizmo.HotControl == null)
			{
				Gizmo.HotControl = Gizmo.HoveredControl;
				RecordUndo();

				if (Gizmo.HotControl.UserData != null)
				{
					Gizmo.Selection.Add(Gizmo.HotControl);
				}

				mouseDownOffset = mousePosition - Gizmo.HotControl.Rect.Center;
			}
		}

		Cursor.Update(Gizmo.HotControl, Gizmo.HoveredControl);

		if (Gizmo.HotControl != null)
		{
			if (Raylib.IsKeyDown(KeyboardKey.KEY_V))
			{
				// Snap to closest other gizmo handle.
				Vector2 closest = gizmos
					.Where(g => !Gizmo.HotControl.Group.Contains(g))
					.Select(x => x.Rect.Center).MinBy(x => Vector2.DistanceSquared(x, mousePosition));

				Raylib.DrawCircleV(closest, 10, Color.RED);
				Coord delta = new Coord(closest - Gizmo.HotControl.Rect.Center);
				Gizmo.HotControl.Apply(delta);
			}
			else
			{
				Vector2 delta = Raylib.GetMouseDelta() + fractionalDragOffset;
				fractionalDragOffset = new Vector2(delta.X - (int)delta.X, delta.Y - (int)delta.Y);
				Gizmo.HotControl.Apply(new Coord(delta));
			}

			if (Raylib.IsKeyReleased(KeyboardKey.KEY_V))
			{
				// Move hot control back to the mouse position.
				Gizmo.HotControl.Apply(
					delta: new Coord(mousePosition - Gizmo.HotControl.Rect.Center - mouseDownOffset));
			}
		}

		if (Gizmo.HotControl != null && Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
		{
			Gizmo.HotControl = null;
			EvaluateChanged();
		}

		foreach (Gizmo gizmo in gizmos.Where(x => x.Parent == null))
		{
			gizmo.Draw();

			if (debugDrawHandles)
			{
				foreach (Gizmo handle in gizmo.AllInGroup())
				{
					handle.Draw();
					Raylib.DrawPixel((int)handle.Rect.Center.X, (int)handle.Rect.Center.Y, Color.RED);
				}
			}
		}

		if (debugDrawHandles)
		{
			Gizmo.HoveredControl?.Draw();
			Gizmo.HotControl?.Draw();
		}
	}

	protected override void DeleteImpl()
	{
		foreach (Gizmo selected in Gizmo.Selection)
		{
			model.Items.Remove((int)selected.UserData);

			foreach (Gizmo g in selected.AllInGroup())
				gizmos.Remove(g);
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