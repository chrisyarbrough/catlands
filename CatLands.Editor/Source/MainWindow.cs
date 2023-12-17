namespace CatLands.Editor;

using ImGuiNET;

public static class MainWindow
{
	private static readonly List<Window> windows = new();
	private static bool showImGuiDemo;
	private static readonly Dictionary<string, Type> windowTypes = new();

	public static void InitializeLayout(List<Func<Window>> windowFactories)
	{
		foreach (Func<Window> factory in windowFactories)
		{
			Window window = factory.Invoke();
			windows.Add(window);
			windowTypes.Add(window.Name, window.GetType());
			window.Load();
		}
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
		{
			Profiler.BeginSample("Update: " + window.Name);
			window.Update();
			Profiler.EndSample();
		}
	}

	public static void Draw()
	{
		DrawToolbar();

		// It's important that the main menu is drawn before the docking area because toggling the menu breaks
		// the docking connection of following windows.

		ImGui.DockSpaceOverViewport(ImGui.GetMainViewport());

		if (showImGuiDemo)
			ImGui.ShowDemoWindow();

		foreach (Window window in windows)
		{
			Profiler.BeginSample("Draw: " + window.Name);
			window.Draw();
			Profiler.EndSample();
		}

		ImGui.End(); // End DockArea
	}

	public static void OnSceneGui()
	{
		foreach (Window window in windows)
		{
			Profiler.BeginSample("OnSceneGui: " + window.Name);
			window.OnSceneGui();
			Profiler.EndSample();
		}
	}

	private static void DrawToolbar()
	{
		ImGui.BeginMainMenuBar();
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

			ImGui.EndMenu();
		}

		ImGui.EndMainMenuBar();
	}
}