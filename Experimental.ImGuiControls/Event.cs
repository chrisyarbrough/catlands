using System.Numerics;
using Raylib_cs;

public readonly struct Event
{
	public readonly EventPhase Phase;
	public readonly Vector2 MousePos;
	public readonly bool IsMousePressed;
	public readonly bool IsMouseReleased;
	
	public Event(EventPhase phase)
	{
		Phase = phase;
		MousePos = Raylib.GetMousePosition();
		IsMousePressed = Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON);
		IsMouseReleased = Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON);
	}
}

public enum EventPhase
{
	Update,
	Draw
}