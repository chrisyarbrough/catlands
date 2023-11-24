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

	public static bool IsPointWithin(this Rectangle rect, Vector2 point)
	{
		return point.X >= rect.X &&
		       point.X <= rect.X + rect.Width &&
		       point.Y >= rect.Y &&
		       point.Y <= rect.Y + rect.Height;
	}
}