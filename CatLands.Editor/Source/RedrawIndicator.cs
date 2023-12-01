namespace CatLands.Editor;

using System.Numerics;
using ImGuiNET;

internal class RedrawIndicator
{
	private int frame;

	private readonly uint[] colors;

	public RedrawIndicator()
	{
		colors = new uint[8];
		for (int i = 0; i < colors.Length; i++)
		{
			float t = i / (float)colors.Length;
			colors[i] = ImGui.ColorConvertFloat4ToU32(new Vector4(t, 1f - t, t, 1f));
		}
	}

	public void AdvanceFrame()
	{
		frame = (frame + 1) % colors.Length;
	}

	public void DrawFrame()
	{
		const int size = 10;
		Vector2 position = ImGui.GetWindowPos() + ImGui.GetWindowSize() - Vector2.One * size - new Vector2(20, 20);
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		drawList.AddCircleFilled(position, size, colors[frame]);
	}
}