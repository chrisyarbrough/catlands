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

	public static Vector2 Position(this Rectangle rect)
	{
		return new Vector2(rect.X, rect.Y);
	}

	public static Vector2 Center(this Rectangle rect)
	{
		return new Vector2(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
	}
}