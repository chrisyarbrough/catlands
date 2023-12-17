namespace CatLands.Editor;

using ImGuiNET;
using Raylib_cs;

internal class MainWindowToolbar
{
	private static readonly Dictionary<string, Type> windowTypes = new()
	{
		{ "Scene View", typeof(SceneView) },
		{ "Hierarchy", typeof(HierarchyWindow) },
		{ "Inspector", typeof(Inspector) },
		{ "Profiler", typeof(ProfilerWindow) },
	};

	public static void Draw(List<Window> windows, ref bool showImGuiDemo)
	{
		ImGui.BeginMainMenuBar();
		if (ImGui.BeginMenu("File"))
		{
			if (ImGui.MenuItem("New"))
				SceneManager.NewMap();

			if (ImGui.MenuItem("Open..."))
				SceneManager.Open();

			if (ImGui.MenuItem("Save", enabled: Map.Current != null))
				SceneManager.Save(Map.Current!);

			ImGui.EndMenu();
		}

		if (ImGui.BeginMenu("Window"))
		{
			foreach ((string key, Type type) in windowTypes)
			{
				if (ImGui.MenuItem(key + "..."))
				{
					if (Activator.CreateInstance(type) is not Window window)
					{
						Console.WriteLine("Failed to create window: " + key);
						continue;
					}

					windows.Add(window);
				}
			}

			ImGui.Separator();

			ImGui.MenuItem("ImGui Demo", string.Empty, ref showImGuiDemo);
			ImGui.MenuItem("Debug Mode", string.Empty, ref DebugMode.Enabled);

			ImGui.Separator();

			if (ImGui.MenuItem("Reset Layout"))
			{
				string iniPath = Path.Combine(AppContext.BaseDirectory, "../EditorAssets/layout-default.ini");
				ImGui.LoadIniSettingsFromDisk(iniPath);
			}

			ImGui.EndMenu();
		}

		if (DebugMode.Enabled)
			DrawFps();

		ImGui.EndMainMenuBar();
	}

	private static void DrawFps()
	{
		float menuBarWidth = ImGui.GetWindowContentRegionMax().X;
		float rightAlignedItemWidth = ImGui.CalcTextSize("999").X;
		ImGui.SameLine(menuBarWidth - rightAlignedItemWidth - ImGui.GetStyle().ItemSpacing.X * 2);
		ImGui.MenuItem(Raylib.GetFPS().ToString(), string.Empty);
	}
}