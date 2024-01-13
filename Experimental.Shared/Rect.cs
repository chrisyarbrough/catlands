using System.Numerics;

/// <summary>
/// A pixel rectangle or 2D axis-aligned bounding box defined by two points.
/// </summary>
/// <remarks>
/// Point 0 and point 1 describe two opposite, but arbitrary corners of the rectangle.
/// It is not required that X0 be less than X1 or Y0 be less than Y1.
/// </remarks>
public struct Rect
{
	public int X0 { get; set; }
	public int X1 { get; set; }
	public int Y0 { get; set; }
	public int Y1 { get; set; }

	public int Width => Math.Abs(X0 - X1);
	public int Height => Math.Abs(Y0 - Y1);

	public int Area => Width * Height;

	public Vector2 Center => new(X0 + (X1 - X0) / 2f, Y0 + (Y1 - Y0) / 2f);

	public Rect Translate(Coord offset)
	{
		X0 += offset.X;
		Y0 += offset.Y;
		X1 += offset.X;
		Y1 += offset.Y;
		return this;
	}

	public Rect(Vector2 a, Vector2 b)
	{
		X0 = (int)a.X;
		X1 = (int)b.X;
		Y0 = (int)a.Y;
		Y1 = (int)b.Y;
	}

	public Rect(float x, float y, float width, float height)
	{
		X0 = (int)x;
		Y0 = (int)y;
		X1 = (int)(x + width);
		Y1 = (int)(y + height);
	}

	public static Rect Handle(Vector2 center, int size)
	{
		return Handle(new Coord(center), (size, size));
	}

	public static Rect Handle(Coord center, (int x, int y) size)
	{
		return Handle((center.X, center.Y), size);
	}

	public static Rect Handle((int x, int y) center, (int x, int y) size)
	{
		return new(center.x - size.x / 2f, center.y - size.y / 2f, size.x, size.y);
	}

	public static Rect FromPointsH(Coord a, Coord b, int handleSize)
	{
		if (b.X < a.X)
			(a, b) = (b, a);

		int handleExtents = handleSize / 2;
		return new Rect(
			a.X + handleExtents,
			a.Y - handleExtents,
			b.X - handleExtents - (a.X + handleExtents),
			handleSize);
	}

	public static Rect FromPointsV(Coord a, Coord b, int handleSize)
	{
		if (b.Y < a.Y)
			(a, b) = (b, a);

		int handleExtents = handleSize / 2;
		return new Rect(
			a.X - handleExtents,
			a.Y + handleExtents,
			handleSize,
			b.Y - handleExtents - (a.Y + handleExtents));
	}

	public static Rect FromPoint(Coord point, int handleSize)
	{
		return Handle((point.X, point.Y), (handleSize, handleSize));
	}

	public static explicit operator Raylib_cs.Rectangle(Rect r)
	{
		return new Raylib_cs.Rectangle(
			Math.Min(r.X0, r.X1),
			Math.Min(r.Y0, r.Y1),
			r.Width,
			r.Height);
	}
}