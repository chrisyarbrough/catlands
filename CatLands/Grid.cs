using System.Numerics;

namespace CatLands;

public static class Grid
{
	public const int TileSourceSize = 16;

	/// <summary>
	/// The size of a grid cell when rendered in the game.
	/// </summary>
	public const int TileRenderSize = TileSourceSize * 4;

	public static Coord CoordToWorld(Coord coord)
	{
		return new Coord(
			coord.X * TileRenderSize,
			coord.Y * TileRenderSize
		);
	}

	public static Coord WorldToCoord(Vector2 screenPosition)
	{
		// FloorToInt for negative numbers.
		return new Coord(
			(int)Math.Floor(screenPosition.X / TileRenderSize),
			(int)Math.Floor(screenPosition.Y / TileRenderSize)
		);
	}
}