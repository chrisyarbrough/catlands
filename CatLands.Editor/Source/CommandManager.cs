namespace CatLands.Editor;

using Raylib_cs;

public static class CommandManager
{
	// TODO: limit stack size.
	private static readonly Stack<Command> undoStack = new();
	private static readonly Stack<Command> redoStack = new();

	public static void Execute(Command command)
	{
		undoStack.Push(command);
		command.Do();
	}

	public static void Undo()
	{
		if (undoStack.Count > 0)
		{
			Command command = undoStack.Pop();
			redoStack.Push(command);
			command.Undo();
		}
	}

	public static void Redo()
	{
		if (redoStack.Count > 0)
		{
			redoStack.Pop().Do();
		}
	}

	public static void Update()
	{
		if (Raylib.IsKeyPressed(KeyboardKey.KEY_Z) &&
		    (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SUPER) || Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL)))
		{
			Undo();
		}
		else if (Raylib.IsKeyPressed(KeyboardKey.KEY_Y) &&
		         (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SUPER) || Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL)))
		{
			Redo();
		}
	}

	public static void Clear()
	{
		undoStack.Clear();
		redoStack.Clear();
	}
}

public abstract class Command
{
	public abstract void Do();
	public abstract void Undo();
}