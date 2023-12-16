using System.Numerics;
using Raylib_cs;

namespace CatLands;

public static class RectangleExtensions
{
	public static float xMin(this Rectangle rect)
	{
		return rect.Width > 0f ? rect.X : rect.X + rect.Width;
	}

	public static float yMin(this Rectangle rect)
	{
		return rect.Height > 0f ? rect.Y : rect.Y + rect.Height;
	}

	public static float xMax(this Rectangle rect)
	{
		return rect.X + rect.Width;
	}

	public static float yMax(this Rectangle rect)
	{
		return rect.Y + rect.Height;
	}

	public static Vector2 Min(this Rectangle rect)
	{
		return new Vector2(rect.xMin(), rect.yMin());
	}

	public static Vector2 Max(this Rectangle rect)
	{
		return new Vector2(rect.xMax(), rect.yMax());
	}

	public static bool IsPointWithin(this Rectangle rect, Vector2 point)
	{
		return point.X >= rect.X &&
		       point.X <= rect.X + rect.Width &&
		       point.Y >= rect.Y &&
		       point.Y <= rect.Y + rect.Height;
	}

	public static bool Encloses(this Rectangle rect, Rectangle other)
	{
		return rect.IsPointWithin(other.Min()) && rect.IsPointWithin(other.Max());
	}

	public static Vector2 Position(this Rectangle rect)
	{
		return new Vector2(rect.X, rect.Y);
	}

	public static Vector2 Center(this Rectangle rect)
	{
		return new Vector2(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
	}

	public static Vector2 TopLeft(this Rectangle rectangle)
	{
		return new Vector2(rectangle.xMin(), rectangle.yMin());
	}

	public static Vector2 TopRight(this Rectangle rectangle)
	{
		return new Vector2(rectangle.xMax(), rectangle.yMin());
	}

	public static Vector2 BottomLeft(this Rectangle rectangle)
	{
		return new Vector2(rectangle.xMin(), rectangle.yMax());
	}

	public static Vector2 BottomRight(this Rectangle rectangle)
	{
		return new Vector2(rectangle.xMax(), rectangle.yMax());
	}

	public static Vector2 Size(this Rectangle rectangle) => new(rectangle.Width, rectangle.Height);

	public static float Area(this Rectangle rectangle)
	{
		return rectangle.Width * rectangle.Height;
	}

	public static Rectangle GrowBy(this Rectangle rectangle, float size)
	{
		return new Rectangle(
			rectangle.X - size,
			rectangle.Y - size,
			rectangle.Width + size * 2,
			rectangle.Height + size * 2);
	}

	public static bool HasSameValues(this Rectangle rectangle, Rectangle other)
	{
		return Math.Abs(rectangle.X - other.X) <= float.Epsilon &&
		       Math.Abs(rectangle.Y - other.Y) <= float.Epsilon &&
		       Math.Abs(rectangle.Width - other.Width) <= float.Epsilon &&
		       Math.Abs(rectangle.Height - other.Height) <= float.Epsilon;
	}
}