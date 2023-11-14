namespace CatLands.Editor;

public struct RectInt
{
	public int X;
	public int Y;
	public int Width;
	public int Height;

	public RectInt(int x, int y, int width, int height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	public bool IsWithin(int x, int y)
	{
		return x >= X &&
		       x <= X + Width &&
		       y >= Y &&
		       y <= Y + Height;
	}
}