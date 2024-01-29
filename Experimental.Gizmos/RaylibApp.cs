namespace Experimental.Gizmos;

using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

public abstract class RaylibApp<T> where T : IModel
{
	protected virtual string Title => GetType().Assembly.GetName().Name;
	protected virtual Color BackgroundColor => Color.DARKGRAY;

	protected T EditModel;

	private Model model;

	public void Run()
	{
		Initialize();

		while (!Raylib.WindowShouldClose())
		{
			Raylib.BeginDrawing();
			Raylib.ClearBackground(BackgroundColor);

			rlImGui.Begin(Raylib.GetFrameTime());
			ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);

			Update(captureInput: ImGui.GetIO() is { WantCaptureMouse: false, WantCaptureKeyboard: false });

			ImGui.End(); // DockArea
			rlImGui.End();

			Raylib.EndDrawing();
		}

		Shutdown();
	}

	protected void Initialize()
	{
		Raylib.SetTraceLogLevel(TraceLogLevel.LOG_ERROR);
		Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE | ConfigFlags.FLAG_VSYNC_HINT);
		Raylib.InitWindow(width: 1280, height: 800, Title);
		Raylib.SetTargetFPS(240);

		rlImGui.Setup(darkTheme: true, enableDocking: true);
		ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.125f, 0.125f, 0.125f, 1f));

		ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;
		ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NoMouseCursorChange;

		model = Model.Load();
		EditModel = (T)Activator.CreateInstance(typeof(T), model);
		EditModel!.Changed += OnModelChanged;
	}

	public void OnModelChanged(bool isDirty)
	{
		Raylib.SetWindowTitle(Title + (isDirty ? "*" : ""));
	}

	protected virtual void Update(bool captureInput) { }

	protected virtual void Shutdown()
	{
		EditModel.Save();
		rlImGui.Shutdown();
		Raylib.CloseWindow();
	}
}