namespace CatLands.Editor;

using ImGuiNET;
using Raylib_cs;
using System.Numerics;
using rlImGui_cs;

public class SceneView : Window
{
	public static SceneView? Current { get; private set; }

	public static bool EnableGrid = true;

	/// <summary>
	/// Indicates that the mouse is in the interactable area of the viewport.
	/// </summary>
	public bool IsMouseOverWindow { get; private set; }

	public float CameraZoom
	{
		get => cameraController.State.Zoom;
		set
		{
			cameraController.SetZoom(value);
			Repaint();
		}
	}

	private readonly CameraController cameraController;

	private RenderTexture2D target;
	private readonly RedrawIndicator redrawIndicator = new();

	private Vector2 gameViewportPos;
	private Vector2 gameViewportSize;
	private int? lastKnownMapVersion;

	public SceneView() : base("Scene View")
	{
		cameraController = new SceneViewCameraController(this);
	}

	public override void Load()
	{
		Console.WriteLine(Directory.GetCurrentDirectory());
		Map.CurrentChanged += OnCurrentChanged;
	}

	public override void Shutdown()
	{
		Map.CurrentChanged -= OnCurrentChanged;
		Raylib.UnloadRenderTexture(target);
	}

	private void OnCurrentChanged()
	{
		Repaint();
	}

	protected override ImGuiWindowFlags SetupWindow()
	{
		ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
		ImGui.SetNextWindowSize(new Vector2(800, 600), ImGuiCond.FirstUseEver);

		return ImGuiWindowFlags.NoScrollbar |
		       ImGuiWindowFlags.NoScrollWithMouse |
		       ImGuiWindowFlags.NoCollapse |
		       ImGuiWindowFlags.MenuBar |
		       ImGuiWindowFlags.NoBringToFrontOnFocus;
	}

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

		UpdateInput(isMouseInHotRect);

		IsMouseOverWindow = ImGui.IsWindowHovered() && isMouseInHotRect;

		Vector2 viewportSize = ImGui.GetContentRegionAvail();
		if (viewportSize != gameViewportSize)
		{
			gameViewportSize = viewportSize;
			Raylib.UnloadRenderTexture(target);
			// Fit to entire viewport. If we wanted to render a fitted fixed resolution, we could set it here.
			const int upscale = 1;
			target = Raylib.LoadRenderTexture((int)viewportSize.X * upscale, (int)viewportSize.Y * upscale);
			Raylib.SetTextureFilter(target.Texture, TextureFilter.TEXTURE_FILTER_POINT);
			Repaint();
		}

		if (Map.Current.ChangeTracker.HasChanged(ref lastKnownMapVersion))
		{
			Repaint();
		}

		if (ImGui.IsWindowHovered())
		{
			Current = this;
		}

		gameViewportPos = ImGui.GetCursorScreenPos();

		rlImGui.ImageRenderTextureFit(target);

		if (DebugMode.Enabled)
			redrawIndicator.DrawFrame();

		ImGui.PopStyleVar();

		CameraWidget.Draw(this);
	}

	public static void RepaintAll()
	{
		foreach (SceneView instance in GetInstances<SceneView>())
			instance.Repaint();
	}

	public void Repaint()
	{
		Raylib.BeginTextureMode(target);
		Raylib.SetTextureFilter(target.Texture, TextureFilter.TEXTURE_FILTER_POINT);
		Raylib.ClearBackground(Color.SKYBLUE);
		cameraController.Begin();

		foreach (GameObject gameObject in Scene.Current.Children)
			gameObject.OnSceneGui(cameraController.State);

		MainWindow.OnSceneGui();

		if (EnableGrid)
			DrawGrid();

		cameraController.End();

		Raylib.EndTextureMode();

		if (IsMouseOverWindow && DebugMode.Enabled)
			DrawMousePosition();

		redrawIndicator.AdvanceFrame();
	}

	private void DrawMenuBar()
	{
		ImGui.BeginMenuBar();
		if (Map.Current != null)
			ImGui.MenuItem("Name: " + Path.GetFileNameWithoutExtension(Map.Current.FilePath), enabled: false);

		ImGui.EndMenuBar();
	}

	private void DrawMousePosition()
	{
		Vector2 mouseWorld = GetMouseWorldPosition();
		HandleUtility.DrawCross(mouseWorld, 10, Color.RED);
	}

	private static void DrawGrid()
	{
		Raylib.DrawLine(-1000, 0, 1000, 0, Color.BLACK);
		Raylib.DrawLine(0, -1000, 0, 1000, Color.BLACK);
		Raylib.DrawCircle(0, 0, 5, Color.RED);
	}

	private void UpdateInput(bool isMouseInHotRect)
	{
		// Normally, right-clicking doesn't focus the window, but we need this to allow for easy panning.
		if (isMouseInHotRect && ImGui.GetIO().MouseClicked[(int)ImGuiMouseButton.Right])
		{
			ImGui.SetWindowFocus(Name);
		}

		cameraController.Update(canBeginInputAction: isMouseInHotRect && ImGui.IsWindowHovered());
	}

	public Vector2 ViewportCenter()
	{
		return new Vector2(gameViewportSize.X / 2f, gameViewportSize.Y / 2f);
	}

	public Vector2 GetMouseWorldPosition()
	{
		Vector2 viewportScreen = GetMouseScreenPosition();
		return Raylib.GetScreenToWorld2D(viewportScreen, cameraController.State);
	}

	public Vector2 GetMouseScreenPosition()
	{
		Vector2 viewportNormalized = GetMouseViewportPositionNormalized();
		Vector2 viewportScreen = viewportNormalized * new Vector2(target.Texture.Width, target.Texture.Height);
		return viewportScreen;
	}

	private Vector2 GetMouseViewportPositionNormalized()
	{
		Vector2 mousePosScreen = Raylib.GetMousePosition();
		Vector2 viewportNormalized = (mousePosScreen - gameViewportPos) / gameViewportSize;
		return viewportNormalized;
	}

	public void ResetCamera()
	{
		cameraController.Reset();
		Repaint();
	}
}