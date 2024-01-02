using System.Numerics;

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
			Handles.fractionalOffset = Vector2.Zero;

			if (Handles.HotControl.HasValue)
				RecordUndo();
		}

		if (current.IsMouseReleased)
		{
			if (Handles.HotControl.HasValue)
				EvaluateChanged();
		}

		foreach ((int id, Rect rect) in model.Items)
		{
			int controlId = Handles.GetControlId();

			if (current.IsMousePressed && Handles.HotControl == controlId)
			{
				selectedIDs.Add(id);
			}

			Vector2 center = new(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
			var delta = Handles.FreeMove(
				controlId, center, new Vector2(rect.Width, rect.Height), selectedIDs.Contains(id));
			Rect r = model.Items[id];
			r.X += (int)delta.X;
			r.Y += (int)delta.Y;
			model.Items[id] = r;

			Vector2 topRight = new(rect.X + rect.Width, rect.Y);
			var trDelta = Handles.FreeMove(topRight, new Vector2(10f, 10f));
			r = model.Items[id];
			r.Width += (int)trDelta.X;
			r.Y += (int)trDelta.Y;
			r.Height -= (int)trDelta.Y;
			model.Items[id] = r;

			Vector2 bottomLeft = new(rect.X, rect.Y + rect.Height);
			var bLDelta = Handles.FreeMove(bottomLeft, new Vector2(10f, 10f));
			r = model.Items[id];
			r.X += (int)bLDelta.X;
			r.Width -= (int)bLDelta.X;
			r.Height += (int)bLDelta.Y;
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