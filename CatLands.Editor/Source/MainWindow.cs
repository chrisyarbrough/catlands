namespace CatLands.Editor;

using ImGuiNET;

internal static class MainWindow
{
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
		{
			Profiler.BeginSample("Update: " + window.Name);
			window.Update();
			Profiler.EndSample();
		}
	}

	public static void Draw()
	{
		MainWindowToolbar.Draw(windows, ref showImGuiDemo);

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
}