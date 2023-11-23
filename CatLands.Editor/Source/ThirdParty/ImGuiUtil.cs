namespace CatLands;

using ImGuiNET;
using System.Numerics;

internal static class ImGuiUtil
{
	public static void TextCentered(string text)
	{
		Vector2 windowWidth = ImGui.GetWindowSize();
		Vector2 textWidth = ImGui.CalcTextSize(text);
		ImGui.SetCursorPos((windowWidth - textWidth) * 0.5f);
		ImGui.Text(text);
	}
}