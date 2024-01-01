namespace CatLands;

using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

public class AppWindow
{
	private readonly App app;

	public AppWindow(App app)
	{
		this.app = app;
	}

	public void Run()
	{
		Initialize();

		while (!Raylib.WindowShouldClose())
		{
			Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.DARKGRAY);
			rlImGui.Begin();

			Cursor.Reset();
			ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
			app.Update();
			ImGui.End(); // DockArea
			Cursor.Draw();

			rlImGui.End();
			Raylib.EndDrawing();
		}

		Shutdown();
	}
	
	public void UpdateSubTitle(string subTitle)
	{
		app.SubTitle = subTitle;
		Raylib.SetWindowTitle(app.CombinedTitle);
	}

	public void SetUnsavedChangesIndicator(bool hasUnsavedChanges)
	{
		string title = app.CombinedTitle;

		if (hasUnsavedChanges)
			title += "*";

		Raylib.SetWindowTitle(title);
	}

	private void Initialize()
	{
		Raylib.SetTraceLogLevel(TraceLogLevel.LOG_WARNING);

		Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE | ConfigFlags.FLAG_VSYNC_HINT);
		Raylib.InitWindow(width: 1280, height: 800, app.BaseTitle);
		Raylib.SetTargetFPS(120);

		rlImGui.Setup(darkTheme: true, enableDocking: true);
		ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.125f, 0.125f, 0.125f, 1f));

		ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;

		app.Initialize();
	}

	private void Shutdown()
	{
		app.Shutdown();
		rlImGui.Shutdown();
		Raylib.CloseWindow();
	}
}