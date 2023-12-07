namespace CatLands;

using Raylib_cs;

public class MouseButtonAction
{
	private readonly MouseButton[] buttons;

	public MouseButtonAction(params MouseButton[] buttons)
	{
		this.buttons = buttons;
	}

	/// <summary>
	/// Returns true if the input action should begin this frame.
	/// </summary>
	public bool Begin()
	{
		foreach (MouseButton button in buttons)
		{
			if (Raylib.IsMouseButtonPressed(button))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Returns true if the input action should end this frame.
	/// </summary>
	public bool End()
	{
		foreach (MouseButton button in buttons)
		{
			if (Raylib.IsMouseButtonReleased(button))
			{
				return true;
			}
		}
		return false;
	}
}