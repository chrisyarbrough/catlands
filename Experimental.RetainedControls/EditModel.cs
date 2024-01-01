using System.Numerics;
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
		foreach (int id in model.Items.Keys)
		{
			AddGizmo(id);
		}
	}

	private void AddGizmo(int id)
	{
		var gizmo = new Gizmo(id,
			() => model.Items[id],
			delta =>
			{
				Rectangle r = model.Items[id];
				r.X += delta.X;
				r.Y += delta.Y;
				model.Items[id] = r;
			});
		gizmos.Add(gizmo);

		var topRight = new Gizmo(null,
			() => new Rectangle(gizmo.Rect.X + gizmo.Rect.Width - 5, gizmo.Rect.Y - 5, 10f, 10f),
			delta =>
			{
				var newRect = gizmo.Rect;
				newRect.Width += delta.X;
				newRect.Y += delta.Y;
				newRect.Height -= delta.Y;
				model.Items[(int)gizmo.UserData] = newRect;
			});
		gizmos.Add(topRight);
		gizmo.Friend = topRight;

		var bottomLeft = new Gizmo(null,
			() => new Rectangle(gizmo.Rect.X - 5, gizmo.Rect.Y + gizmo.Rect.Height - 5, 10f, 10f),
			delta =>
			{
				var newRect = gizmo.Rect;
				newRect.X += delta.X;
				newRect.Width -= delta.X;
				newRect.Height += delta.Y;
				model.Items[(int)gizmo.UserData] = newRect;
			});
		gizmos.Add(bottomLeft);
		topRight.Friend = bottomLeft;
	}

	public override int AddItem(Vector2 position, float size)
	{
		int id = base.AddItem(position, size);
		AddGizmo(id);
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
				if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), gizmo.Rect))
				{
					Rectangle rect = gizmo.Rect;
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

		foreach (Gizmo gizmo in gizmos)
		{
			gizmo.Draw();
		}

		if (Gizmo.HotControl != null)
		{
			Gizmo.HotControl.Move(Raylib.GetMouseDelta());
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

			foreach (Gizmo g in selected.Group())
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