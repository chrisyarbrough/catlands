namespace CatLands.Editor;

using System.Numerics;
using Raylib_cs;

public static class HandleUtility
{
	public static void DrawCross(Vector2 position, int size, Color color)
	{
		Vector2 left = position;
		left.X -= size;
		Vector2 right = position;
		right.X += size;
		Vector2 top = position;
		top.Y -= size;
		Vector2 bottom = position;
		bottom.Y += size;
		Raylib.DrawLineV(left, right, color);
		Raylib.DrawLineV(bottom, top, color);
	}
}