using System.Numerics;

namespace CatLands;

using Raylib_cs;

public class MapDisplay : GameObject
{
	private readonly MapTextures mapTextures = new();

	private Map? map => Map.Current;

	public override void OnSceneGui()
	{
		if (map == null)
			return;

		mapTextures.UpdateLoadState();

		for (int i = 0; i < map.LayerCount; i++)
		{
			Layer layer = map.GetLayer(i);
			Texture2D texture = mapTextures.Get(layer.TexturePath);
			DrawLayer(layer, texture);
		}
	}

	private void DrawLayer(Layer layer, Texture2D texture)
	{
		const int sourceSize = Grid.TileSourceSize;
		const float destSize = Grid.TileRenderSize;

		foreach ((Coord coord, int tileId) in layer.Tiles)
		{
			int xTileCount = texture.Width / sourceSize;

			if (xTileCount == 0)
				continue;

			var sourceRect = new Rectangle(
				x: tileId % xTileCount * sourceSize,
				y: tileId / xTileCount * sourceSize,
				sourceSize,
				sourceSize);

			var destinationRect = new Rectangle(
				coord.X * destSize, coord.Y * destSize, destSize, destSize);

			Raylib.DrawTexturePro(
				texture,
				sourceRect,
				destinationRect,
				Vector2.Zero,
				0f,
				Color.WHITE);
		}
	}
}