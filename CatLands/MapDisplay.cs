namespace CatLands;

using Raylib_cs;
using SpriteEditor;
using System.Numerics;

public class MapDisplay : GameObject
{
	private static Map? Map => Map.Current;

	private static TileRenderInfo preview;
	private static bool drawPreview;

	public override void OnSceneGui()
	{
		if (Map == null)
			return;

		MapTextures.UpdateLoadState();

		for (int i = 0; i < Map.LayerCount; i++)
		{
			Layer layer = Map.GetLayer(i);
			if (layer.IsVisible)
				DrawLayer(layer, i);
		}
	}

	public static void SetPreview(TileRenderInfo preview)
	{
		MapDisplay.preview = preview;
		drawPreview = true;
	}

	public static void RefreshPreview(TileRenderInfo preview)
	{
		if (drawPreview)
		{
			MapDisplay.preview = preview;
		}
	}

	public static void RemovePreview()
	{
		drawPreview = false;
	}

	private void DrawLayer(Layer layer, int layerId)
	{
		SpriteAtlas atlas = MapTextures.GetAtlas(layer.TexturePath);

		foreach ((Coord coord, int tileId) in layer.Tiles)
		{
			TileRenderInfo.DrawTile(atlas, tileId, coord);
		}

		if (drawPreview && preview.LayerId == layerId)
		{
			preview.Draw(atlas);
		}
	}
}

public readonly record struct TileRenderInfo(int LayerId, Coord Coord, int TileId)
{
	public void Draw(SpriteAtlas atlas)
	{
		DrawTile(atlas, TileId, Coord);
	}

	public static void DrawTile(SpriteAtlas atlas, int tileId, Coord coord)
	{
		const float destSize = Grid.TileRenderSize;
		var destinationRect = new Rectangle(coord.X * destSize, coord.Y * destSize, destSize, destSize);

		Raylib.DrawTexturePro(
			atlas.Texture,
			source: atlas.SpriteRects[tileId],
			destinationRect,
			Vector2.Zero,
			0f,
			Color.WHITE);
	}
}