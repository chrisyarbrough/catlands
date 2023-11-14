using System.Numerics;

namespace CatLands;

using Raylib_cs;

public class MapDisplay
{
	private readonly Map map;
	public Texture2D[] textures;

	public MapDisplay(Map map)
	{
		this.map = map;
		textures = new Texture2D[map.Tilesets.Length];
	}

	public void LoadAssets()
	{
		for (int i = 0; i < textures.Length; i++)
			textures[i] = Raylib.LoadTexture(map.Tilesets[i]);
	}

	public void Render()
	{
		const int sourceSize = Grid.TileSourceSize;
		const float destSize = Grid.TileRenderSize;

		foreach (Tile tile in map.Tiles)
		{
			int xTileCount = textures[tile.TilesetId].width / sourceSize;

			var sourceRect = new Rectangle(
				x: tile.Id % xTileCount * sourceSize,
				y: tile.Id / xTileCount * sourceSize,
				sourceSize,
				sourceSize);

			var destinationRect = new Rectangle(
				tile.Coord.X * destSize, tile.Coord.Y * destSize, destSize, destSize);

			Raylib.DrawTexturePro(
				textures[tile.TilesetId],
				sourceRect,
				destinationRect,
				Vector2.Zero,
				0f,
				Color.WHITE);
		}
	}
}