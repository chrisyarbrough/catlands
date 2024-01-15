using System.Numerics;
using Experimental.RetainedControls;
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
	private Vector2 originalOppositeControl;
	private DottedLine dottedLine;

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

				if (Gizmo.HotControl.Parent != null)
				{
					originalOppositeControl = Gizmo.HotControl.OppositeGizmo.Rect.Center;
					dottedLine = new DottedLine(Gizmo.HotControl.Rect.Center, Gizmo.HotControl.OppositeCorner);
				}
			}
		}

		if (Gizmo.HotControl != null && Gizmo.HotControl.IsCorner && Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
		{
			dottedLine.Draw(Gizmo.HotControl.Rect.Center, Gizmo.HotControl.OppositeCorner);
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

				Raylib.DrawCircleV(closest, 7, Color.WHITE);
				Gizmo.HotControl.SetPosition(closest);
			}
			else
			{
				if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT) && Gizmo.HotControl.IsCorner)
				{
					Vector2 snappedPosition = dottedLine.ClosestPointTo(Raylib.GetMousePosition() - mouseDownOffset);

					Raylib.DrawCircleV(snappedPosition, 5, Color.YELLOW);
					Gizmo.HotControl.SetPosition(snappedPosition);

					if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_ALT))
					{
						Vector2 oppositePosition = 2 * dottedLine.Center - snappedPosition;
						oppositePosition = dottedLine.ClosestPointTo(oppositePosition);
						Gizmo.HotControl.OppositeGizmo.SetPosition(oppositePosition);
						Raylib.DrawCircleV(oppositePosition, 5, Color.SKYBLUE);
					}
				}
				else if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_ALT))
				{
					Vector2 delta = Raylib.GetMouseDelta() + fractionalDragOffset;
					fractionalDragOffset = new Vector2(delta.X - (int)delta.X, delta.Y - (int)delta.Y);
					Gizmo.HotControl.Apply(new Coord(delta));
					
					Vector2 oppositePosition = 2 * dottedLine.Center - Gizmo.HotControl.Rect.Center;
					
					Gizmo.HotControl.OppositeGizmo.SetPosition(oppositePosition);
				}
				else
				{
					Vector2 delta = Raylib.GetMouseDelta() + fractionalDragOffset;
					fractionalDragOffset = new Vector2(delta.X - (int)delta.X, delta.Y - (int)delta.Y);
					Gizmo.HotControl.Apply(new Coord(delta));
				}
			}

			if (Raylib.IsKeyReleased(KeyboardKey.KEY_V) || Raylib.IsKeyReleased(KeyboardKey.KEY_LEFT_SHIFT))
			{
				// Move hot control back to the mouse position.
				Gizmo.HotControl.Apply(
					delta: new Coord(mousePosition - mouseDownOffset - Gizmo.HotControl.Rect.Center));
			}

			if (Raylib.IsKeyReleased(KeyboardKey.KEY_LEFT_ALT))
			{
				Gizmo.HotControl.OppositeGizmo.SetPosition(originalOppositeControl);
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
				foreach (Gizmo handle in gizmo.Group)
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

			foreach (Gizmo g in selected.Group)
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