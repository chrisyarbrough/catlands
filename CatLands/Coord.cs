namespace CatLands;

public struct Coord
{
	public int X;
	public int Y;

	public Coord(int x, int y)
	{
		X = x;
		Y = y;
	}

	public override string ToString()
	{
		return $"{X}|{Y}";
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
}