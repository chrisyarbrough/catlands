using System.Numerics;

public abstract class EditModelBase
{
	public event Action<bool> Changed;

	private readonly Stack<string> undoStack = new();
	private string lastSavedState;

	protected Model model;

	public EditModelBase(Model model)
	{
		this.model = model;
		lastSavedState = model.Serialize();
	}

	public void Save()
	{
		lastSavedState = model.Save();
		Changed?.Invoke(false);
	}
	
	public virtual int AddItem(Rect rect)
	{
		RecordUndo();
		int id = model.Add(rect);
		EvaluateChanged();
		return id;
	}

	public void RecordUndo()
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
	
	public void EvaluateChanged()
	{
		bool isDirty = lastSavedState != model.Serialize();
		Changed?.Invoke(isDirty);
	}

	public void DeleteSelected()
	{
		RecordUndo();
		DeleteImpl();
		EvaluateChanged();
	}

	protected abstract void DeleteImpl();
}