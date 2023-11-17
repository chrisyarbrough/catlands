namespace CatLands.Editor;

using ImGuiNET;
using Raylib_cs;
using System.Numerics;

public class SceneView : Window
{
	private Camera2D camera = new(Vector2.Zero, Vector2.Zero, rotation: 0f, zoom: 1f);
	private uint renderTargetId;
	private RenderTexture2D target;

	private Vector2 gameViewportPos;
	private Vector2 gameViewportSize;

	private Scene scene;

	public SceneView(Scene scene) : base("Scene View")
	{
		target = Raylib.LoadRenderTexture(Raylib.GetRenderWidth() / 2, Raylib.GetRenderHeight() / 2);
		renderTargetId = target.texture.id;
		this.scene = scene;
	}

	public override void Render()
	{
		if (!isOpen)
			return;

		DrawSceneViewWindow();

		Raylib.BeginTextureMode(target);
		Raylib.ClearBackground(Color.SKYBLUE);
		Raylib.BeginMode2D(camera);


		foreach (GameObject gameObject in scene.Children)
			gameObject.Update();

		DrawGrid();
		Vector2 mouseWorld = GetMouseWorldPosition();
		Raylib.DrawCircle((int)mouseWorld.X, (int)mouseWorld.Y, 3, Color.RED);


		Raylib.EndMode2D();
		Raylib.EndTextureMode();


	}

	private static void DrawGrid()
	{
		Raylib.DrawLine(-1000, 0, 1000, 0, Color.BLACK);
		Raylib.DrawLine(0, -1000, 0, 1000, Color.BLACK);
		Raylib.DrawCircle(0, 0, 5, Color.RED);
	}

	private bool isFocused;
	public static SceneView Current;
	private bool isOpen = true;

	private void DrawSceneViewWindow()
	{
		ImGui.SetNextWindowSize(new Vector2(800, 600), ImGuiCond.FirstUseEver);
		if (ImGui.Begin(Name, ref isOpen, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
		{
			isFocused = ImGui.IsWindowFocused();
			if (ImGui.IsWindowHovered())
				Current = this;

			UpdateInput();

			Vector2 viewportSize = ImGui.GetContentRegionAvail();
			if (viewportSize != gameViewportSize)
			{
				gameViewportSize = viewportSize;
				Raylib.UnloadRenderTexture(target);
				target = Raylib.LoadRenderTexture((int)viewportSize.X, (int)viewportSize.Y);
				renderTargetId = target.texture.id;
			}

			gameViewportPos = ImGui.GetCursorScreenPos();

			var texturePointer = new IntPtr(renderTargetId);

			// Draw the scene as an image but overlay with an invisible button to capture the mouse in order to
			// avoid dragging the window when interacting with the scene.
			Vector2 pos = ImGui.GetCursorPos();
			ImGui.Image(texturePointer, viewportSize, new Vector2(0, 1), new Vector2(1, 0));
			ImGui.SetCursorPos(pos);
			ImGui.InvisibleButton(string.Empty, viewportSize);

			OnSceneGui(ImGui.IsItemHovered());
		}

		ImGui.End();
	}

	private void UpdateInput()
	{
		if (isFocused == false)
			return;

		if (Raylib.IsKeyPressed(KeyboardKey.KEY_C))
		{
			RecenterToOrigin();
		}

		if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT) ||
		    Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_MIDDLE))
		{
			Vector2 delta = Raylib.GetMouseDelta();
			delta *= -1.0f / camera.zoom;
			camera.target += delta;
		}
	}

	private void RecenterToOrigin()
	{
		camera.target = Vector2.Zero;
		camera.offset = new Vector2(gameViewportSize.X / 2f, gameViewportSize.Y / 2f);
		camera.zoom = 1f;
	}

	private void OnSceneGui(bool isActive)
	{
		foreach (GameObject gameObject in scene.Children)
			gameObject.OnGui(isActive);

		if (isActive)
		{
			// Normally, right-clicking doesn't focus the window, but we need this to allow for easy panning.
			if (ImGui.GetIO().MouseClicked[(int)ImGuiMouseButton.Right])
			{
				ImGui.SetWindowFocus(Name);
			}
		}
	}

	private Vector2 GetMouseScreenPosition()
	{
		Vector2 viewportNormalized = GetMouseViewportPositionNormalized();
		Vector2 viewportScreen = viewportNormalized * new Vector2(target.texture.width, target.texture.height);
		return viewportScreen;
	}

	public Vector2 GetMouseViewportPositionNormalized()
	{
		Vector2 mousePosScreen = Raylib.GetMousePosition();
		Vector2 viewportNormalized = (mousePosScreen - gameViewportPos) / gameViewportSize;
		return viewportNormalized;
	}

	public Vector2 GetMouseWorldPosition()
	{
		Vector2 viewportScreen = GetMouseScreenPosition();
		return Raylib.GetScreenToWorld2D(viewportScreen, camera);
	}
}