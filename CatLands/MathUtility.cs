namespace CatLands;

using System.Numerics;

public class MathUtility
{
	public static float Lerp(float a, float b, float t)
	{
		return a + (b - a) * t;
	}

	public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
	{
		return new Vector2(
			a.X + (b.X - a.X) * t,
			a.Y + (b.Y - a.Y) * t
		);
	}

	public static Vector2 InvLerp(Vector2 a, Vector2 b, float t)
	{
		return new Vector2(
			(t - a.X) / (b.X - a.X),
			(t - a.Y) / (b.Y - a.Y)
		);
	}
	
	/// <summary>
	/// Fits a given size into a frame, maintaining the original aspect ratio.
	/// </summary>
	/// <param name="size">The size of the rectangle that is fitted.</param>
	/// <param name="frameSize">The size of the rectangle that is fitted into.</param>
	/// <returns>The size of the fitted rectangle.</returns>
	public static Vector2 FitTo(Vector2 size, Vector2 frameSize)
	{
		float scaleX = frameSize.X / size.X;
		float scaleY = frameSize.Y / size.Y;
		float minScale = Math.Min(scaleX, scaleY);

		return new Vector2(size.X * minScale, size.Y * minScale);
	}
}