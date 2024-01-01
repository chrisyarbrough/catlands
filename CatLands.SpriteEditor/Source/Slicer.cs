namespace CatLands.SpriteEditor;

using System.Numerics;
using ImGuiNET;
using Raylib_cs;

internal class Slicer
{
	private readonly SpriteAtlasViewModel viewModel;
	private int tileSizeX = 16;
	private int tileSizeY = 16;

	public Slicer(SpriteAtlasViewModel viewModel)
	{
		this.viewModel = viewModel;
	}

	public void DrawGui()
	{
		ImGui.TextUnformatted("Tile Size");
		ImGui.InputInt("X", ref tileSizeX);
		ImGui.InputInt("Y", ref tileSizeY);

		tileSizeX = Math.Clamp(tileSizeX, 1, int.MaxValue);
		tileSizeY = Math.Clamp(tileSizeY, 1, int.MaxValue);

		if (ImGui.Button("Slice", new Vector2(ImGui.GetContentRegionAvail().X, 0f)))
		{
			viewModel.GenerateSpriteRects(Slice);
		}
	}

	private IEnumerable<Rect> Slice(Texture2D texture)
	{
		int cols = texture.Width / tileSizeX;
		int rows = texture.Height / tileSizeY;

		for (int y = 0; y < rows; y++)
		{
			for (int x = 0; x < cols; x++)
			{
				yield return new Rect(x * tileSizeX, y * tileSizeY, tileSizeX, tileSizeY);
			}
		}
	}
}