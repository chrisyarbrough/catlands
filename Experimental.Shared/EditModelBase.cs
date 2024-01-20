public abstract class EditModelBase
{
	public event Action<bool> Changed;

	protected Dictionary<int, Rect> Rects => model.Rects;

	private readonly Stack<string> undoStack = new();
	private string lastSavedState;

	private Model model;

	protected EditModelBase(Model model)
	{
		this.model = model;
		lastSavedState = model.Serialize();
	}

	public void Save()
	{
		lastSavedState = model.Save();
		Changed?.Invoke(false);
	}
	
	public virtual int AddRect(Rect rect)
	{
		RecordUndo();
		int id = model.Add(rect);
		EvaluateChanged();
		return id;
	}

	protected void RecordUndo()
	{
		string yaml = model.Serialize();
		if (undoStack.Count == 0 || yaml != undoStack.Peek())
			undoStack.Push(yaml);
	}

	public virtual void Undo()
	{
		if (undoStack.Any())
		{
			string yaml = undoStack.Pop();
			model = Model.Deserialize(yaml);
			EvaluateChanged();
		}
	}

	protected void EvaluateChanged()
	{
		bool isDirty = lastSavedState != model.Serialize();
		Changed?.Invoke(isDirty);
	}

	public void DeleteSelected()
	{
		RecordUndo();
		DeleteSelectedRects();
		EvaluateChanged();
	}

	protected abstract void DeleteSelectedRects();
}