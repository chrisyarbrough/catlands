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
		Gizmo.HoveredControl = null;
		float smallestDistance = float.MaxValue;
		float smallestArea = float.MaxValue;

		if (Gizmo.HotControl == null)
		{
			foreach (Gizmo gizmo in gizmos)
			{
				if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), (Rectangle)gizmo.Rect))
				{
					Rect rect = gizmo.Rect;
					Vector2 center = new(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);

					float distance = Vector2.Distance(Raylib.GetMousePosition(), center);

					float area = rect.Width * rect.Height;
					if (distance < smallestDistance || area < smallestArea)
					{
						smallestArea = area;
						smallestDistance = distance;
						Gizmo.HoveredControl = gizmo;
					}
				}
			}
		}

		if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
		{
			Gizmo.Selection.Clear();

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
		
		Cursor.SetFromGizmo(Gizmo.HoveredControl, Gizmo.HotControl);

		foreach (Gizmo gizmo in gizmos)
		{
			gizmo.Draw();
		}

		if (Gizmo.HotControl != null)
		{
			Gizmo.HotControl.Apply((Offset)Raylib.GetMouseDelta());
		}

		if (Gizmo.HotControl != null && Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
		{
			Gizmo.HotControl = null;
			EvaluateChanged();
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