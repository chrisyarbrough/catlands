namespace CatLands;

using System.ComponentModel;

[TypeConverter(typeof(CoordConverter))]
public readonly struct Coord
{
	public readonly int X;
	public readonly int Y;

	public Coord(int x, int y)
	{
		X = x;
		Y = y;
	}

	public override string ToString()
	{
		return $"{X}|{Y}";
	}

	public static Coord Parse(string s)
	{
		string[] split = s.Split("|");
		return new Coord(
			x: int.Parse(split[0]),
			y: int.Parse(split[1]));
	}

	public bool Equals(Coord other)
	{
		return X == other.X && Y == other.Y;
	}

	public override bool Equals(object? obj)
	{
		return obj is Coord other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(X, Y);
	}

	public static bool operator ==(Coord a, Coord b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(Coord a, Coord b)
	{
		return !a.Equals(b);
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