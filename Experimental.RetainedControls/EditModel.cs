using System.Numerics;
using Raylib_cs;

public class EditModel : EditModelBase
{
	private readonly List<Gizmo> gizmos = new();
	private readonly GizmoFactory gizmoFactory = new();

	private static readonly bool debugDrawHandles = true;

	/// <summary>
	/// Carries over the fractional part of the drag movement because only the integer part is applied to the model.
	/// </summary>
	private Vector2 fractionalOffset;

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
		if (Gizmo.HotControl == null)
		{
			Gizmo.HoveredControl = SelectionStrategy.FindHoveredControl(
				Raylib.GetMousePosition(), gizmos);
		}

		if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
		{
			Gizmo.Selection.Clear();
			fractionalOffset = Vector2.Zero;

			if (Gizmo.HoveredControl != null && Gizmo.HotControl == null)
			{
				Gizmo.HotControl = Gizmo.HoveredControl;
				RecordUndo();

				if (Gizmo.HotControl.UserData != null)
				{
					Gizmo.Selection.Add(Gizmo.HotControl);
				}
			}
		}

		Cursor.Update(Gizmo.HotControl, Gizmo.HoveredControl);

		if (Gizmo.HotControl != null)
		{
			Vector2 delta = Raylib.GetMouseDelta() + fractionalOffset;
			fractionalOffset = new Vector2(delta.X - (int)delta.X, delta.Y - (int)delta.Y);
			Gizmo.HotControl.Apply(new Coord(delta));
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