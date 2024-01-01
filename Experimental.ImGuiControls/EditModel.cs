using System.Numerics;
using Raylib_cs;

internal class EditModel : EditModelBase
{
	private static readonly HashSet<int> selectedIDs = new();

	public EditModel(Model model) : base(model)
	{
	}

	public void OnGUI(Event current)
	{
		if (current.IsMousePressed)
		{
			selectedIDs.Clear();

			if (Handles.HotControl.HasValue)
				RecordUndo();
		}

		if (current.IsMouseReleased)
		{
			if (Handles.HotControl.HasValue)
				EvaluateChanged();
		}

		foreach ((int id, Rectangle rect) in model.Items)
		{
			int controlId = Handles.GetControlId();

			if (current.IsMousePressed && Handles.HotControl == controlId)
			{
				selectedIDs.Add(id);
			}

			Vector2 center = new(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
			var delta = Handles.FreeMove(
				controlId, center, new Vector2(rect.Width, rect.Height), selectedIDs.Contains(id));
			Rectangle r = model.Items[id];
			r.X += delta.X;
			r.Y += delta.Y;
			model.Items[id] = r;

			Vector2 topRight = new(rect.X + rect.Width, rect.Y);
			var trDelta = Handles.FreeMove(topRight, new Vector2(10f, 10f));
			r = model.Items[id];
			r.Width += trDelta.X;
			r.Y += trDelta.Y;
			r.Height -= trDelta.Y;
			model.Items[id] = r;

			Vector2 bottomLeft = new(rect.X, rect.Y + rect.Height);
			var bLDelta = Handles.FreeMove(bottomLeft, new Vector2(10f, 10f));
			r = model.Items[id];
			r.X += bLDelta.X;
			r.Width -= bLDelta.X;
			r.Height += bLDelta.Y;
			model.Items[id] = r;
		}
	}

	protected override void DeleteImpl()
	{
		foreach (int id in selectedIDs)
		{
			model.Items.Remove(id);
		}

		selectedIDs.Clear();
		Handles.HotControl = null;
	}
}