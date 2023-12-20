namespace CatLands.SpriteEditor;

using System.Numerics;
using ImGuiNET;
using Raylib_cs;

public class Slicer
{
	private int tileSizeX = 16;
	private int tileSizeY = 16;

	public void DrawGui(SpriteAtlas spriteAtlas)
	{
		ImGui.TextUnformatted("Tile Size");
		ImGui.InputInt("X", ref tileSizeX);
		ImGui.InputInt("Y", ref tileSizeY);

		tileSizeX = Math.Clamp(tileSizeX, 1, int.MaxValue);
		tileSizeY = Math.Clamp(tileSizeY, 1, int.MaxValue);

		if (ImGui.Button("Slice", new Vector2(ImGui.GetContentRegionAvail().X, 0f)))
		{
			spriteAtlas.GenerateSpriteRects(PerformSlicing);
		}
	}

	private void PerformSlicing(Texture2D texture, List<Rect> spriteRects)
	{
		int cols = texture.Width / tileSizeX;
		int rows = texture.Height / tileSizeY;

		for (int y = 0; y < rows; y++)
		{
			for (int x = 0; x < cols; x++)
			{
				var tile = new Rect(x * tileSizeX, y * tileSizeY, tileSizeX, tileSizeY);
				spriteRects.Add(tile);
			}
		}
	}
}