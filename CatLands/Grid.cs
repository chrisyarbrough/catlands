using System.Numerics;

namespace CatLands;

public static class Grid
{
	/// <summary>
	/// The size of a grid cell when rendered in the game.
	/// </summary>
	public const int TileRenderSize = 16 * 4;

	public static Coord CoordToWorld(Coord coord)
	{
		return new Coord(
			coord.X * TileRenderSize,
			coord.Y * TileRenderSize
		);
	}

	public static Coord WorldToCoord(Vector2 screenPosition)
	{
		return new Coord(
			(int)Math.Floor(screenPosition.X / TileRenderSize),
			(int)Math.Floor(screenPosition.Y / TileRenderSize)
		);
	}
}