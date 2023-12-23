namespace CatLands.Editor;

using ImGuiNET;

internal class PrefsWindow : Window
{
	public PrefsWindow() : base("Prefs")
	{
	}

	protected override void DrawContent()
	{
		foreach ((string key, string value) in Prefs.All)
		{
			ImGui.LabelText(value, key);
		}
	}
}