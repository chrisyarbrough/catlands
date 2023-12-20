namespace CatLands.Editor;

using System.Numerics;
using Raylib_cs;
using ImGuiNET;
using rlImGui_cs;

internal class Program
{
	public static string AssetsDirectory => Path.Combine(Directory.GetCurrentDirectory(), "Assets");

	private const string version = "1.0";

	private static readonly Scene scene = new();

	private static void Main(string[] args)
	{
		if (args.Length == 0)
		{
			Console.WriteLine("Usage: <ProjectPath> (<MapFilePath>)");
			return;
		}

		Directory.SetCurrentDirectory(args[0]);
		Console.WriteLine("Working directory: " + Directory.GetCurrentDirectory());

		Prefs.Load();
		SceneManager.TryLoadInitialScene(args);

		LogBuffer.Initialize();

		Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE | ConfigFlags.FLAG_VSYNC_HINT);
		Raylib.InitWindow(width: 1280, height: 800, title: $"CatLands Editor {version}");
		Raylib.SetTargetFPS(120);

		rlImGui.Setup(darkTheme: true, enableDocking: true);
		ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.125f, 0.125f, 0.125f, 1f));

		ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;

		var mapDisplay = new MapDisplay();

		// scene.AddChild(mapDisplay);
		scene.Setup();

		MainWindow.AddToolbar(MainWindowToolbar.Draw);
		MainWindow.InitializeLayout(new List<Func<Window>>
		{
			() => new SceneView(),
			() => new HierarchyWindow(),
			() => new Inspector(),
			() => new LogWindow(),
			() => new TileBrushWindow(),
			() => new LayersWindow(),
		});

		while (!Raylib.WindowShouldClose())
		{
			Profiler.BeginFrame();
			Profiler.BeginSample("Main");

			Profiler.BeginSample("Scene Update");
			scene.Update();
			Profiler.EndSample();

			MainWindow.Update();
			CommandManager.Update();

			Raylib.BeginDrawing();
			rlImGui.Begin();

			MainWindow.DrawWindows();

			Profiler.BeginSample("Draw: ImGui");
			rlImGui.End();
			Profiler.EndSample();

			Profiler.BeginSample("Draw: Raylib");
			Raylib.EndDrawing();
			Profiler.EndSample();

			Profiler.EndSample();
		}

		scene.Shutdown();
		MainWindow.Shutdown();

		rlImGui.Shutdown();
		Raylib.CloseWindow();
	}
}