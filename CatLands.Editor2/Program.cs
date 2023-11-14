namespace CatLands.Editor;

using System.Numerics;
using Raylib_cs;
using ImGuiNET;
using rlImGui_cs;

internal static class MapEditor2
{
	public static bool WantsMouseCapture()
	{
		// TODO: Offset by approx. title bar on y.
		Vector2 viewportNormalized = GetMouseViewportPositionNormalized();
		bool v = viewportNormalized.X is >= 0 and <= 1 &&
		       viewportNormalized.Y is >= 0 and <= 1;
		ImGui.LabelText("Argh", viewportNormalized.ToString());
		return v;
	}
	
	private static uint renderTargetId;
	private static RenderTexture2D target;

	private static Vector2 gameViewportPos;
	private static Vector2 gameViewportSize;

	private static void Main()
	{
		Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
		Raylib.InitWindow(width: 1280, height: 800, title: "Map Editor");
		Raylib.SetTargetFPS(120);

		target = Raylib.LoadRenderTexture(Raylib.GetRenderWidth() / 2, Raylib.GetRenderHeight() / 2);
		renderTargetId = target.texture.id;

		rlImGui.Setup(enableDocking: true);
		rlImGui.CancelMouseCapture = WantsMouseCapture;

		camera.target += new Vector2(0, -20);

		while (!Raylib.WindowShouldClose())
		{
			Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.BLUE);
			rlImGui.Begin();

			Raylib.BeginTextureMode(target);
			Raylib.ClearBackground(Color.BEIGE);
			Raylib.BeginMode2D(camera);
			Raylib.DrawCircle(0, 0, 10, Color.RED);
			Raylib.DrawCircle(600, 0, 10, Color.RED);

			Vector2 viewportScreen = GetMouseScreenPosition();
			Raylib.DrawCircle((int)viewportScreen.X, (int)viewportScreen.Y, 5, Color.GOLD);

			Raylib.EndMode2D();
			Raylib.EndTextureMode();

			DrawDock();

			ImGui.ShowDemoWindow();
			ImGui.Begin("Inspector");
			ImGui.Button("Bla");
			ImGui.End();

			DrawSceneView();

			ImGui.End(); // End Dock
			rlImGui.End();

			Raylib.EndDrawing();
		}

		rlImGui.Shutdown();
		Raylib.CloseWindow();
	}

	private static Vector2 GetMouseScreenPosition()
	{
		Vector2 viewportNormalized = GetMouseViewportPositionNormalized();
		Vector2 viewportScreen = viewportNormalized * new Vector2(target.texture.width, target.texture.height);
		return viewportScreen;
	}

	private static Vector2 GetMouseViewportPositionNormalized()
	{
		Vector2 mousePosScreen = Raylib.GetMousePosition();
		Vector2 mousePosWorld = Raylib.GetScreenToWorld2D(mousePosScreen, camera);
		Vector2 viewportNormalized = (mousePosWorld - gameViewportPos) / gameViewportSize;
		return viewportNormalized;
	}

	public static float Lerp(float a, float b, float t)
	{
		return a + (b - a) * t;
	}
	
	public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
	{
		return new Vector2(
			a.X + (b.X - a.X) * t,
			a.Y + (b.Y - a.Y) * t
		);
	}
	
	public static Vector2 InvLerp(Vector2 a, Vector2 b, float t)
	{
		return new Vector2(
			(t - a.X) / (b.X - a.X),
			(t - a.Y) / (b.Y - a.Y)
		);
	}

	private static void DrawDock()
	{
		ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always);
		ImGui.SetNextWindowSize(new Vector2(Raylib.GetRenderWidth(), Raylib.GetRenderHeight()));
		ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
		ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
		ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
		ImGui.Begin("DockSpace Demo", ImGuiWindowFlags.MenuBar |
		                              ImGuiWindowFlags.NoDocking |
		                              ImGuiWindowFlags.NoTitleBar |
		                              ImGuiWindowFlags.NoCollapse |
		                              ImGuiWindowFlags.NoResize |
		                              ImGuiWindowFlags.NoMove |
		                              ImGuiWindowFlags.NoBringToFrontOnFocus |
		                              ImGuiWindowFlags.NoNavFocus);
		ImGui.PopStyleVar(3);
		ImGui.DockSpace(ImGui.GetID("MyDockSpace"));
	}

	private static Camera2D camera = new(Vector2.Zero, Vector2.Zero, rotation: 0f, zoom: 1f);

	private static void DrawSceneView()
	{
		ImGui.Begin("Scene View", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
		Vector2 windowSize = GetLargestSizeForViewport();
		gameViewportSize = windowSize;


		Vector2 windowPos = GetCenteredPositionForViewport(windowSize);
		ImGui.SetCursorPos(windowPos);

		gameViewportPos = ImGui.GetCursorScreenPos();
		gameViewportPos.X -= ImGui.GetScrollX();
		gameViewportPos.Y -= ImGui.GetScrollY();

		IntPtr p = new IntPtr(renderTargetId);
		ImGui.Image(p, windowSize, new Vector2(0, 1), new Vector2(1, 0));

		ImGui.End();
	}

	private static Vector2 GetLargestSizeForViewport()
	{
		Vector2 windowSize = ImGui.GetContentRegionAvail();
		windowSize.X -= ImGui.GetScrollX();
		windowSize.Y -= ImGui.GetScrollY();

		const float aspectRatio = (16f / 9f);
		float aspectWidth = windowSize.X;
		float aspectHeight = aspectWidth / aspectRatio;

		if (aspectHeight > windowSize.Y)
		{
			// Pillarbox mode
			aspectHeight = windowSize.Y;
			aspectWidth = aspectHeight * aspectRatio;
		}

		return new Vector2(aspectWidth, aspectHeight);
	}

	private static Vector2 GetCenteredPositionForViewport(Vector2 aspectSize)
	{
		Vector2 windowSize = ImGui.GetContentRegionAvail();
		windowSize.X -= ImGui.GetScrollX();
		windowSize.Y -= ImGui.GetScrollY();

		float viewportX = (windowSize.X / 2f) - (aspectSize.X / 2f);
		float viewportY = (windowSize.Y / 2f) - (aspectSize.Y / 2f);
		return new Vector2(viewportX + ImGui.GetCursorPosX(), viewportY + ImGui.GetCursorPosY());
	}
}