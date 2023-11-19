namespace CatLands.Editor;

using System.Numerics;
using Raylib_cs;
using ImGuiNET;
using rlImGui_cs;

internal static class Program
{
	public const string Version = "1.0";

	private static readonly Scene scene = new();

	private static void Main(string[] args)
	{
		MapFileProvider.OpenOrGetFromCommandLine(args);
		
		LogBuffer.Initialize();

		Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE | ConfigFlags.FLAG_VSYNC_HINT);
		Raylib.InitWindow(width: 1280, height: 800, title: $"CatLands Editor {Version}");
		Raylib.SetTargetFPS(120);

		rlImGui.Setup(darkTheme: true, enableDocking: true);
		ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.125f, 0.125f, 0.125f, 1f));

		ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;

		var mapDisplay = new MapDisplay();
		mapDisplay.LoadAssets();

		scene.AddChild(mapDisplay);
		scene.Setup();

		MainWindow.InitializeLayout(new List<Window>
		{
			new SceneView(),
			new HierarchyWindow(),
			new Inspector(),
			new LogWindow(),
			new TileBrushWindow(mapDisplay)
		});

		while (!Raylib.WindowShouldClose())
		{
			scene.Update();
			MainWindow.Update();
			CommandManager.Update();

			Raylib.BeginDrawing();
			rlImGui.Begin();

			MainWindow.Draw();
			scene.OnSceneGui();

			rlImGui.End();
			Raylib.EndDrawing();
		}

		scene.Shutdown();
		MainWindow.Shutdown();

		rlImGui.Shutdown();
		Raylib.CloseWindow();
	}
}