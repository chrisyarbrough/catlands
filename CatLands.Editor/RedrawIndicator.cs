namespace CatLands.Editor;

using System.Numerics;
using ImGuiNET;

internal class RedrawIndicator
{
	private int frame;

	private readonly uint[] colors;

	public RedrawIndicator()
	{
		colors = new uint[10];
		for (int i = 0; i < colors.Length; i++)
		{
			float t = i / (float)colors.Length;
			colors[i] = ImGui.ColorConvertFloat4ToU32(new Vector4(t, 1f - t, t, 0.5f));
		}
	}

	public void AdvanceFrame()
	{
		frame = (frame + 1) % colors.Length;
	}

	public void DrawFrame()
	{
		var rectangleSize = new Vector2(25, 25);
		var offset = new Vector2(20, 20);
		Vector2 windowPos = ImGui.GetWindowPos();
		Vector2 windowSize = ImGui.GetWindowSize();

		// Position the indicator in the top-right corner of the content region.
		Vector2 rectMin = windowPos + windowSize - rectangleSize - offset;
		Vector2 rectMax = windowPos + windowSize - offset;

		// uint rectColor = 
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		drawList.AddRectFilled(rectMin, rectMax, colors[frame]);
	}
}