namespace CatLands;

using Raylib_cs;
using System.Numerics;

public class MapDisplay : GameObject
{
	private static Map? Map => Map.Current;

	private static readonly List<TileRenderInfo> previews = new();
	private static bool drawPreview;

	public void Update(float deltaTime)
	{
		if (Map == null)
			return;

		MapTextures.UpdateLoadState();

		for (int i = 0; i < Map.LayerCount; i++)
		{
			Layer layer = Map.GetLayer(i);
			if (layer.IsVisible)
				DrawLayer(layer, i, new Camera2D()); // TODO
		}
	}

	public static void AddPreview(TileRenderInfo preview)
	{
		previews.Add(preview);
		drawPreview = true;
	}

	public static void SetPreview(TileRenderInfo preview)
	{
		previews.Clear();
		previews.Add(preview);
		drawPreview = true;
	}

	public static void RefreshPreview(TileRenderInfo preview)
	{
		if (drawPreview)
		{
			previews.Clear();
			previews.Add(preview);
		}
	}

	public static void ClearPreviews()
	{
		drawPreview = false;
		previews.Clear();
	}

	private void DrawLayer(Layer layer, int layerId, Camera2D camera)
	{
		SpriteAtlas atlas = MapTextures.GetAtlas(layer.TexturePath);

		foreach ((Coord coord, int tileId) in layer.Tiles)
		{
			TileRenderInfo.DrawTile(atlas, tileId, coord);
		}

		// TODO: It would be better if the preview would not draw on top, but actually replace the tiles temporarily.
		if (drawPreview)
		{
			foreach (TileRenderInfo preview in previews)
			{
				if (preview.LayerId == layerId)
					preview.Draw(atlas, camera);
			}
		}
	}
}

public readonly record struct TileRenderInfo(int LayerId, Coord Coord, int TileId)
{
	public void Draw(SpriteAtlas atlas, Camera2D camera)
	{
		DrawTile(atlas, TileId, Coord);
		Coord screenPos = Grid.CoordToWorld(Coord);
		var rect = new Rectangle(screenPos.X, screenPos.Y, Grid.TileRenderSize, Grid.TileRenderSize);
		float scaleFactor = 1f / camera.Zoom;
		float lineWidth = 2f * scaleFactor;
		Raylib.DrawRectangleLinesEx(rect, lineWidth, Color.RED);
	}

	public static void DrawTile(SpriteAtlas atlas, int tileId, Coord coord)
	{
		const float destSize = Grid.TileRenderSize;
		var destinationRect = new Rectangle(coord.X * destSize, coord.Y * destSize, destSize, destSize);

		if (tileId < 0 || tileId >= atlas.SpriteRects.Count)
			return;

		Raylib.DrawTexturePro(
			atlas.Texture,
			source: atlas.SpriteRects[tileId],
			destinationRect,
			Vector2.Zero,
			0f,
			Color.WHITE);
	}
}