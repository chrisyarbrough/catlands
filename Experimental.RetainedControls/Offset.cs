using System.Numerics;

public readonly struct Offset
{
	public readonly int X;
	public readonly int Y;

	public Offset(Vector2 delta)
	{
		X = (int)delta.X;
		Y = (int)delta.Y;
	}
}