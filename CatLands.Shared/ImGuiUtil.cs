namespace CatLands;

using ImGuiNET;
using System.Numerics;

public static class ImGuiUtil
{
	public static void TextCentered(string text)
	{
		Vector2 windowWidth = ImGui.GetWindowSize();
		Vector2 textWidth = ImGui.CalcTextSize(text);
		ImGui.SetCursorPos((windowWidth - textWidth) * 0.5f);
		ImGui.Text(text);
	}

	public static void HelpMarker(string text)
	{
		ImGui.TextDisabled("(?)");
		if (ImGui.BeginItemTooltip())
		{
			ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
			ImGui.TextUnformatted(text);
			ImGui.PopTextWrapPos();
			ImGui.EndTooltip();
		}
	}
}