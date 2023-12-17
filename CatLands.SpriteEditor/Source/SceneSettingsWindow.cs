namespace CatLands.SpriteEditor;

using ImGuiNET;

internal class SceneSettingsWindow : Window
{
	private static readonly List<(string, Action)> settings = new();

	public SceneSettingsWindow() : base("Scene Settings")
	{
	}

	public static void Add(string label, Action drawFunction)
	{
		settings.Add((label, drawFunction));
		settings.Sort((a, b) => string.Compare(a.Item1, b.Item1, StringComparison.Ordinal));
	}

	protected override void DrawContent()
	{
		foreach ((string label, Action draw) in settings)
		{
			if (ImGui.CollapsingHeader(label, ImGuiTreeNodeFlags.DefaultOpen))
			{
				ImGui.Indent();
				draw.Invoke();
				ImGui.Unindent();
			}
		}
	}
}