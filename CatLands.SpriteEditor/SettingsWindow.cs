namespace CatLands.SpriteEditor;

using ImGuiNET;

public static class SettingsWindow
{
	private static readonly List<(string, Action)> settings = new();

	public static void Add(string label, Action drawFunction)
	{
		settings.Add((label, drawFunction));
		settings.Sort((a, b) => string.Compare(a.Item1, b.Item1, StringComparison.Ordinal));
	}

	public static void Draw()
	{
		if (ImGui.Begin("Settings"))
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

		ImGui.End();
	}
}