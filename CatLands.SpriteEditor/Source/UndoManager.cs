namespace CatLands.SpriteEditor;

public class UndoManager
{
	public event Action? UndoRedoPerformed;

	private readonly Stack<string> undoStack = new();
	private readonly Stack<string> redoStack = new();

	private readonly IMementoOwner owner;

	private string? lastCleanState;

	public UndoManager(IMementoOwner owner)
	{
		this.owner = owner;
		MarkClean();
	}

	public void RecordSnapshot()
	{
		string memento = owner.CreateMemento();
		undoStack.Push(memento);
		redoStack.Clear();
	}

	public void Undo()
	{
		if (undoStack.Count > 0)
		{
			redoStack.Push(owner.CreateMemento());
			owner.RestoreState(undoStack.Pop());
			UndoRedoPerformed?.Invoke();
		}
	}

	public void Redo()
	{
		if (redoStack.Count > 0)
		{
			undoStack.Push(owner.CreateMemento());
			owner.RestoreState(redoStack.Pop());
			UndoRedoPerformed?.Invoke();
		}
	}

	public void MarkClean() => lastCleanState = owner.CreateMemento();

	public bool IsDirty()
	{
		string newState = owner.CreateMemento();
		return lastCleanState != newState;
	}
}