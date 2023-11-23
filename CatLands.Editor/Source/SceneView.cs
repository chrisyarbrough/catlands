namespace CatLands.Editor;

using ImGuiNET;
using Raylib_cs;
using System.Numerics;
using rlImGui_cs;

public class SceneView : Window
{
	public static SceneView? Current { get; private set; }

	/// <summary>
	/// Indicates that the mouse is in the interactable area of the viewport.
	/// </summary>
	public bool IsMouseOverWindow { get; private set; }

	private Camera2D camera = new(Vector2.Zero, Vector2.Zero, rotation: 0f, zoom: 1f);
	private RenderTexture2D target;
	private readonly RedrawIndicator redrawIndicator = new();
	private bool pendingRedraw;
	private bool loadingRenderTexture;

	private Vector2 gameViewportPos;
	private Vector2 gameViewportSize;

	private const bool VisualizeMousePosition = true;

	public SceneView() : base("Scene View")
	{
	}

	public override void Setup()
	{
		Map.CurrentChanged += OnCurrentChanged;
	}

	public override void Shutdown()
	{
		Map.CurrentChanged -= OnCurrentChanged;
		Raylib.UnloadRenderTexture(target);
	}

	private void OnCurrentChanged()
	{
		pendingRedraw = true;
	}

	protected override ImGuiWindowFlags SetupWindow()
	{
		ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
		ImGui.SetNextWindowSize(new Vector2(800, 600), ImGuiCond.FirstUseEver);

		return ImGuiWindowFlags.NoScrollbar |
		       ImGuiWindowFlags.NoScrollWithMouse |
		       ImGuiWindowFlags.NoCollapse |
		       ImGuiWindowFlags.MenuBar;
	}

	private static bool AnyInputGiven()
	{
		for (int key = 0; key <= (int)KeyboardKey.KEY_KB_MENU; key++)
		{
			if (Raylib.IsKeyPressed((KeyboardKey)key))
				return true;
		}

		for (int mouseButton = 0; mouseButton <= (int)MouseButton.MOUSE_BUTTON_BACK; mouseButton++)
		{
			if (Raylib.IsMouseButtonPressed((MouseButton)mouseButton))
				return true;
		}

		if (Raylib.GetMouseDelta().LengthSquared() > 0f)
			return true;

		return false;
	}

	private int? lastRenderedVersion;

	protected override void DrawContent()
	{
		DrawMenuBar();

		if (Map.Current == null)
		{
			ImGuiUtil.TextCentered("No map loaded. Use the file menu to create to open or create one.");
			return;
		}

		// Move the interactable area a tiny bit away from the edges of the viewport to avoid interfering with
		// window resizing or tab dragging, etc.
		const int inset = 4;
		bool isMouseInHotRect = ImGui.IsMouseHoveringRect(
			ImGui.GetCursorScreenPos() + new Vector2(inset, inset),
			ImGui.GetCursorScreenPos() + ImGui.GetContentRegionAvail() - new Vector2(inset, inset));

		if (isMouseInHotRect)
			UpdateInput();

		Vector2 viewportSize = ImGui.GetContentRegionAvail();
		if (viewportSize != gameViewportSize)
		{
			gameViewportSize = viewportSize;
			Raylib.UnloadRenderTexture(target);
			// Fit to entire viewport. If we wanted to render a fitted fixed resolution, we could set it here.
			target = Raylib.LoadRenderTexture((int)viewportSize.X, (int)viewportSize.Y);
			loadingRenderTexture = true;
		}


		if (AnyInputGiven() && ImGui.IsWindowHovered())
		{
			pendingRedraw = true;
		}

		if (loadingRenderTexture && Raylib.IsRenderTextureReady(target))
		{
			loadingRenderTexture = false;
			pendingRedraw = true;
		}

		if (pendingRedraw || Map.Current.ChangeTracker.HasChanged(ref lastRenderedVersion))
		{
			pendingRedraw = false;

			Raylib.BeginTextureMode(target);
			Raylib.ClearBackground(Color.SKYBLUE);
			Raylib.BeginMode2D(camera);

			foreach (GameObject gameObject in Scene.Current.Children)
				gameObject.OnSceneGui();

			MainWindow.OnSceneGui();

			DrawGrid();

			if (isMouseInHotRect)
				DrawMousePosition();

			Raylib.EndMode2D();
			Raylib.EndTextureMode();
			redrawIndicator.AdvanceFrame();
		}

		if (ImGui.IsWindowHovered())
		{
			Current = this;
		}


		gameViewportPos = ImGui.GetCursorScreenPos();

		rlImGui.ImageRenderTextureFit(target);
		redrawIndicator.DrawFrame();

		OnSceneGui(isMouseInHotRect);

		ImGui.PopStyleVar();
	}

	private void DrawMenuBar()
	{
		ImGui.BeginMenuBar();
		if (ImGui.MenuItem("Recenter", enabled: Map.Current != null))
		{
			RecenterToOrigin();
		}

		ImGui.EndMenuBar();
	}

	private void DrawMousePosition()
	{
		if (VisualizeMousePosition)
		{
			Vector2 mouseWorld = GetMouseWorldPosition();
			Vector2 left = mouseWorld;
			left.X -= 10;
			Vector2 right = mouseWorld;
			right.X += 10;
			Vector2 top = mouseWorld;
			top.Y -= 10;
			Vector2 bottom = mouseWorld;
			bottom.Y += 10;
			Raylib.DrawLineV(left, right, Color.RED);
			Raylib.DrawLineV(bottom, top, Color.RED);
		}
	}

	private static void DrawGrid()
	{
		Raylib.DrawLine(-1000, 0, 1000, 0, Color.BLACK);
		Raylib.DrawLine(0, -1000, 0, 1000, Color.BLACK);
		Raylib.DrawCircle(0, 0, 5, Color.RED);
	}

	private void UpdateInput()
	{
		if (Raylib.IsKeyPressed(KeyboardKey.KEY_C))
		{
			RecenterToOrigin();
		}

		if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT) ||
		    Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_MIDDLE))
		{
			Vector2 delta = Raylib.GetMouseDelta();
			delta *= -1.0f / camera.Zoom;
			camera.Target += delta;
		}

		if (MathF.Abs(Raylib.GetMouseWheelMove()) > 0)
		{
			Vector2 mouseWorldPos = GetMouseWorldPosition();

			camera.Offset = GetMouseScreenPosition();

			// Set the target to match, so that the camera maps the world space point 
			// under the cursor to the screen space point under the cursor at any zoom
			camera.Target = mouseWorldPos;

			const float zoomSpeed = 0.125f;
			float zoomFactor = (float)Math.Log(camera.Zoom + 1, 10) * zoomSpeed;
			camera.Zoom = Math.Clamp(camera.Zoom + Raylib.GetMouseWheelMove() * zoomFactor, 0.1f, 10f);
		}
	}

	private void RecenterToOrigin()
	{
		camera.Target = Vector2.Zero;
		camera.Offset = new Vector2(gameViewportSize.X / 2f, gameViewportSize.Y / 2f);
		camera.Zoom = 1f;
	}

	private void OnSceneGui(bool isActive)
	{
		this.IsMouseOverWindow = isActive;

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
		Vector2 viewportScreen = viewportNormalized * new Vector2(target.Texture.Width, target.Texture.Height);
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