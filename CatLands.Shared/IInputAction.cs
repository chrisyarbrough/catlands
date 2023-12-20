namespace CatLands;

using Raylib_cs;

public class InputComposite : IInputAction
{
	private readonly IInputAction[] inputActions;

	public InputComposite(params IInputAction[] inputActions)
	{
		this.inputActions = inputActions;
	}

	public static IInputAction Compose<T>(Func<T, IInputAction> factory, params T[] args)
	{
		var inputActions = new IInputAction[args.Length];
		for (int i = 0; i < args.Length; i++)
			inputActions[i] = factory(args[i]);

		return new InputComposite(inputActions);
	}

	public bool IsStarted() => inputActions.Any(x => x.IsStarted());
	public bool IsDown() => inputActions.Any(x => x.IsDown());
	public bool IsEnded() => inputActions.Any(x => x.IsEnded());
}

public class ModifierAction : IInputAction
{
	private readonly IInputAction inputAction;
	private readonly IInputAction modifierAction;

	public ModifierAction(IInputAction inputAction, IInputAction modifierAction)
	{
		this.inputAction = inputAction;
		this.modifierAction = modifierAction;
	}

	public bool IsStarted() => inputAction.IsStarted() && modifierAction.IsDown();

	public bool IsDown() => inputAction.IsDown() && modifierAction.IsDown();

	public bool IsEnded() => inputAction.IsEnded() || modifierAction.IsEnded();
}

public interface IInputAction
{
	/// <summary>
	/// Returns true during the frame in which this input action has started.
	/// </summary>
	bool IsStarted();

	/// <summary>
	/// Returns true for all frames that this input action is active.
	/// </summary>
	bool IsDown();

	/// <summary>
	/// Return true during the frame in which this input action has ended.
	/// </summary>
	bool IsEnded();
}

public abstract class InputAction<T> : IInputAction
{
	/// <summary>
	/// The key or button that triggers this action.
	/// </summary>
	private readonly T argument;

	private readonly Func<T, CBool> isPressedFunction;
	private readonly Func<T, CBool> isDownFunction;
	private readonly Func<T, CBool> isReleasedFunction;

	protected InputAction(
		Func<T, CBool> isPressedFunction,
		Func<T, CBool> isDownFunction,
		Func<T, CBool> isReleasedFunction,
		T argument)
	{
		this.isPressedFunction = isPressedFunction;
		this.isDownFunction = isDownFunction;
		this.isReleasedFunction = isReleasedFunction;
		this.argument = argument;
	}

	public bool IsStarted() => isPressedFunction(argument);

	public bool IsDown() => isDownFunction(argument);

	public bool IsEnded() => isReleasedFunction(argument);
}

public class MouseButtonAction : InputAction<MouseButton>
{
	public MouseButtonAction(MouseButton button)
		: base(
			Raylib.IsMouseButtonPressed,
			Raylib.IsMouseButtonDown,
			Raylib.IsMouseButtonReleased,
			button)
	{
	}

	public static IInputAction Pan => new InputComposite(
		new MouseButtonAction(MouseButton.MOUSE_BUTTON_MIDDLE),
		new MouseButtonAction(MouseButton.MOUSE_BUTTON_RIGHT));
}

public class KeyboardKeyAction : InputAction<KeyboardKey>
{
	public KeyboardKeyAction(KeyboardKey key)
		: base(
			Raylib.IsKeyPressed,
			Raylib.IsKeyDown,
			Raylib.IsKeyReleased,
			key)
	{
	}
}