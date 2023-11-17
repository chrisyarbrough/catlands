namespace CatLands.Editor;

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
}