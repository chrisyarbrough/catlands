using System.Numerics;

public struct Coord
{
	public int X { get; set; }
	public int Y { get; set; }

	public Coord(int x, int y)
	{
		X = x;
		Y = y;
	}

	public Coord(Vector2 v)
	{
		X = (int)v.X;
		Y = (int)v.Y;
	}

	public static implicit operator Vector2(Coord c)
	{
		return new Vector2(c.X, c.Y);
	}


	public static Coord operator +(Coord a, Coord b)
	{
		return new Coord(a.X + b.X, a.Y + b.Y);
	}
	
	public static Coord operator -(Coord a, Coord b)
	{
		return new Coord(a.X - b.X, a.Y - b.Y);
	}
}