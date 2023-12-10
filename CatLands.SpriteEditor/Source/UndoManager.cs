using CatLands.SpriteEditor;

public static class UndoManager
{
	private static readonly Stack<string> undoStack = new();
	private static readonly Stack<string> redoStack = new();

	public static void RecordSnapshot(SpriteAtlas spriteAtlas)
	{
		string json = spriteAtlas.GetMemento();
		undoStack.Push(json);
		redoStack.Clear();
	}

	public static void Undo(SpriteAtlas spriteAtlas)
	{
		if (undoStack.Count > 0)
		{
			redoStack.Push(spriteAtlas.GetMemento());
			spriteAtlas.SetMemento(undoStack.Pop());
			SaveDirtyTracker.EvaluateDirty(spriteAtlas);
		}
	}

	public static void Redo(SpriteAtlas spriteAtlas)
	{
		if (redoStack.Count > 0)
		{
			undoStack.Push(spriteAtlas.GetMemento());
			spriteAtlas.SetMemento(redoStack.Pop());
			SaveDirtyTracker.EvaluateDirty(spriteAtlas);
		}
	}
}