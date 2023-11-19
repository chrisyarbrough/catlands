using System.Numerics;

namespace CatLands;

using Raylib_cs;

public class MapDisplay : GameObject
{
	public readonly Texture2D[] Textures = new Texture2D[map.Tilesets.Length];

	private static Map map => Map.Current!;

	public void LoadAssets()
	{
		for (int i = 0; i < Textures.Length; i++)
			Textures[i] = Raylib.LoadTexture(map.Tilesets[i]);
	}

	public override void OnSceneGui()
	{
		const int sourceSize = Grid.TileSourceSize;
		const float destSize = Grid.TileRenderSize;

		foreach (Tile tile in map.Tiles)
		{
			int xTileCount = Textures[tile.TilesetId].Width / sourceSize;

			var sourceRect = new Rectangle(
				x: tile.Id % xTileCount * sourceSize,
				y: tile.Id / xTileCount * sourceSize,
				sourceSize,
				sourceSize);

			var destinationRect = new Rectangle(
				tile.Coord.X * destSize, tile.Coord.Y * destSize, destSize, destSize);

			Raylib.DrawTexturePro(
				Textures[tile.TilesetId],
				sourceRect,
				destinationRect,
				Vector2.Zero,
				0f,
				Color.WHITE);
		}
	}
}