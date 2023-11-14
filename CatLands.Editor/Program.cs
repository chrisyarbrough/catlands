namespace CatLands.Editor;

using System.Numerics;
using Raylib_cs;
using ImGuiNET;
using rlImGui_cs;

internal static class MapEditor
{
	private static void Main(string[] args)
	{
		if (args.Length == 0)
		{
			Console.WriteLine("Usage: <mapFilePath>");
			return;
		}

		Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
		Raylib.InitWindow(width: 1280, height: 800, title: "Map Editor");
		Raylib.SetTargetFPS(60);

		Map? map = SaveFileManager.Load(filePath: args[0]);

		if (map == null)
			return;

		var mapDisplay = new MapDisplay(map);
		mapDisplay.LoadAssets();

		var sceneView = new SceneView();
		var brush = new Brush(map, mapDisplay);

		RenderTexture2D target = Raylib.LoadRenderTexture(Raylib.GetRenderWidth(), Raylib.GetRenderHeight());
		

		rlImGui.Setup(enableDocking: true);

		while (!Raylib.WindowShouldClose())
		{
			unsafe
			{
				
				
				
				rlImGui.Begin();
				
				// DrawDockingArea();

				Raylib.BeginDrawing();
				Raylib.ClearBackground(Color.RAYWHITE);
				

				ImGui.Begin("Scene View", ImGuiWindowFlags.None);
				IntPtr textureId = new IntPtr(target.texture.id);
				Vector2 sceneViewSize = ImGui.GetWindowSize();
				ImGui.Image(textureId, sceneViewSize);

				target.texture.width = (int)sceneViewSize.X;
				target.texture.height = (int)sceneViewSize.Y;
				Raylib.BeginTextureMode(target);
				
				Raylib.ClearBackground(new Color(100, 149, 237, 255));
				// Raylib.BeginScissorMode((int)pos.X, (int)pos.Y, (int)panelRect.X, (int)panelRect.Y);
				sceneView.Begin();
				mapDisplay.Render();
				// if (!ImGui.GetIO().WantCaptureMouse)
				{
					sceneView.Update();
					brush.Update(map, sceneView);
				}
				// Raylib.EndScissorMode();
				sceneView.End();
				Raylib.EndTextureMode();
				
				
				// brush.DrawToolsPanel();
			
				
				ImGui.End();

				rlImGui.End();
				
				
				Raylib.EndDrawing();
			}
		}

		rlImGui.Shutdown();
		Raylib.CloseWindow();
	}

	private static unsafe void DrawDockingArea()
	{
		ImGuiViewport* viewport = ImGui.GetMainViewport();
		ImGui.SetNextWindowPos(viewport->WorkPos);
		ImGui.SetNextWindowSize(viewport->WorkSize);
		ImGui.SetNextWindowViewport(viewport->ID);
		ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
		ImGui.Begin("DockSpace Demo", ImGuiWindowFlags.MenuBar |
		                              ImGuiWindowFlags.NoDocking |
		                              ImGuiWindowFlags.NoTitleBar |
		                              ImGuiWindowFlags.NoCollapse |
		                              ImGuiWindowFlags.NoResize |
		                              ImGuiWindowFlags.NoMove |
		                              ImGuiWindowFlags.NoBringToFrontOnFocus |
		                              ImGuiWindowFlags.NoNavFocus |
		                              ImGuiWindowFlags.NoBackground);
		ImGui.PopStyleVar(3);
		uint dockspace_id = ImGui.GetID("MyDockSpace");
		ImGui.DockSpace(dockspace_id, new Vector2(0.0f, 0.0f), ImGuiDockNodeFlags.PassthruCentralNode);
		ImGui.End();
	}
}