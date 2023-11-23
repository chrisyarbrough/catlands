namespace CatLands.Editor;

using ImGuiNET;
using System.Reflection;

internal static class MainWindow
{
	private static readonly Dictionary<string, Type> windowTypes = new()
	{
		{ "Scene View", typeof(SceneView) },
		{ "Hierarchy", typeof(HierarchyWindow) },
		{ "Inspector", typeof(Inspector) },
	};

	private static readonly List<Window> windows = new();
	private static bool showImGuiDemo;

	public static void InitializeLayout(List<Window> windows)
	{
		MainWindow.windows.AddRange(windows);
		foreach (Window window in windows)
			window.Setup();
	}

	public static void Shutdown()
	{
		foreach (Window window in windows)
			window.Shutdown();
	}

	public static void Update()
	{
		windows.RemoveAll(x => !x.IsOpen);

		foreach (Window window in windows)
			window.Update();
	}

	public static void Draw()
	{
		DrawMainMenu();

		// It's important that the main menu is drawn before the docking area because toggling the menu breaks
		// the docking connection of following windows.

		ImGui.DockSpaceOverViewport(ImGui.GetMainViewport());

		if (showImGuiDemo)
			ImGui.ShowDemoWindow();

		foreach (Window window in windows)
			window.Draw();

		ImGui.End(); // End DockArea
	}

	private static void DrawMainMenu()
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
			if (ImGui.MenuItem("Reset Layout"))
			{
				string iniPath = Path.Combine(AppContext.BaseDirectory, "../EditorAssets/layout-default.ini");
				ImGui.LoadIniSettingsFromDisk(iniPath);
			}

			ImGui.EndMenu();
		}

		ImGui.EndMainMenuBar();
	}

	public static void OnSceneGui()
	{
		foreach (Window window in windows)
			window.OnSceneGui();
	}
}