using System.Numerics;
using CatLands.SpriteEditor;
using ImGuiNET;
using NativeFileDialogSharp;
using Raylib_cs;
using rlImGui_cs;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

string textureFilePath = string.Empty;
SpriteAtlas? spriteAtlas = default;
Texture2D checkerTexture = default;
int checkerBaseValue = 128;
int checkerAlpha = 0;
var camera = new Camera2D(Vector2.Zero, Vector2.Zero, 0f, 1f);
float cameraMinZoom = 0.5f;
float cameraMaxZoom = 20f;

int tileSizeX = 16;
int tileSizeY = 16;

const string configFilePath = "LastFilePath.txt";

if (args.Length > 0)
{
	textureFilePath = args[0];
}
else if (File.Exists(configFilePath))
{
	textureFilePath = File.ReadAllText(configFilePath);
}

Initialize();

while (!Raylib.WindowShouldClose())
{
	Raylib.BeginDrawing();
	Raylib.ClearBackground(Color.DARKGRAY);
	rlImGui.Begin();
	Update();
	rlImGui.End();
	Raylib.EndDrawing();
}

Shutdown();
return;

void Initialize()
{
	Image icon = Raylib.LoadImage("Icon.png");
	Raylib.SetWindowIcon(icon);
	Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE | ConfigFlags.FLAG_VSYNC_HINT);
	Raylib.InitWindow(width: 1280, height: 800, title: "CatLands SpriteEditor");
	Raylib.SetTargetFPS(120);

	rlImGui.Setup(darkTheme: true, enableDocking: true);
	ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.125f, 0.125f, 0.125f, 1f));

	ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;

	if (File.Exists(textureFilePath))
	{
		spriteAtlas = SpriteAtlas.Load(textureFilePath);
		checkerTexture = CreateCheckerTexture();
	}

	ResetCamera();
}

void Update()
{
	if (Raylib.IsKeyDown(KeyboardKey.KEY_SPACE) == false)
		DrawMenu();
	DrawScene();
}

void DrawMenu()
{
	ImGui.BeginMainMenuBar();
	if (ImGui.BeginMenu("File"))
	{
		if (ImGui.MenuItem("Open"))
		{
			string? defaultPath = File.Exists(textureFilePath)
				? Path.GetDirectoryName(textureFilePath)
				: Directory.GetCurrentDirectory();

			DialogResult result = Dialog.FileOpen(defaultPath: defaultPath);
			if (result.IsOk)
			{
				textureFilePath = result.Path;
				File.WriteAllText(configFilePath, textureFilePath);
				spriteAtlas = SpriteAtlas.Load(textureFilePath);
				ResetCamera();
			}
		}

		if (ImGui.MenuItem("Save", enabled: spriteAtlas != null))
		{
			spriteAtlas!.Save();
		}

		ImGui.EndMenu();
	}

	if (ImGui.MenuItem("Slice", enabled: spriteAtlas != null))
	{
		ImGui.OpenPopup("SlicePopup");
	}

	if (ImGui.BeginPopup("SlicePopup"))
	{
		ImGui.LabelText("Tile Size", string.Empty);
		ImGui.BeginGroup();
		ImGui.Indent();
		ImGui.InputInt("X", ref tileSizeX);
		ImGui.InputInt("Y", ref tileSizeY);
		ImGui.Unindent();
		ImGui.EndGroup();
		if (ImGui.Button("Slice"))
		{
			spriteAtlas!.Slice(tileSizeX, tileSizeY);
		}

		ImGui.EndPopup();
	}

	if (ImGui.MenuItem("Reset Camera"))
		ResetCamera();

	ImGui.EndMainMenuBar();
}

void DrawScene()
{
	Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
	Raylib.BeginMode2D(camera);
	UpdateCamera();
	Vector2 mousePos = Raylib.GetMousePosition();

	if (spriteAtlas != null)
	{
		ImGui.SliderInt("Background Alpha", ref checkerAlpha, 0, 255);
		if (ImGui.SliderInt("Background Value", ref checkerBaseValue, 0, 255))
			checkerTexture = CreateCheckerTexture();

		Raylib.DrawTexture(checkerTexture, 0, 0, new Color(255, 255, 255, checkerAlpha));
		Raylib.DrawTexture(spriteAtlas.Texture, 0, 0, Color.WHITE);

		ImGui.LabelText(string.Empty, "Gizmo Settings");
		ImGui.Indent();
		ImGui.Checkbox("Snap to pixel", ref RectangleGizmo.SnapToPixel);
		ImGui.Unindent();

		ImGui.LabelText(string.Empty, "Camera Zoom");
		ImGui.Indent();
		bool changed1 = ImGui.DragFloat("Min", ref cameraMinZoom, 0.05f, 0.01f, 1f);
		bool changed2 = ImGui.DragFloat("Max", ref cameraMaxZoom, 0.05f, 1f, 100f);
		if (changed1 || changed2)
			UpdateCameraZoom();
		ImGui.Unindent();

		for (int i = 0; i < spriteAtlas.SpriteRects.Count; i++)
		{
			spriteAtlas.SpriteRects[i] = RectangleGizmo.Draw(spriteAtlas.SpriteRects[i], i, mousePos, camera);
		}

		if (RectangleGizmo.SelectedControlId != -1)
		{
			Rectangle rect = spriteAtlas.SpriteRects[RectangleGizmo.SelectedControlId];
			ImGui.LabelText("Rect", $"{rect.X} {rect.Y} {rect.Width} {rect.Height}");
		}

		if ((Raylib.IsKeyPressed(KeyboardKey.KEY_DELETE) || Raylib.IsKeyPressed(KeyboardKey.KEY_BACKSPACE)) &&
		    RectangleGizmo.SelectedControlId != -1)
		{
			spriteAtlas.SpriteRects.RemoveAt(RectangleGizmo.SelectedControlId);
			RectangleGizmo.SelectedControlId = -1;
		}
	}

	Raylib.EndMode2D();
}

void UpdateCamera()
{
	if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT) ||
	    Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_MIDDLE))
	{
		Vector2 delta = Raylib.GetMouseDelta();
		delta *= -1.0f / camera.Zoom;
		camera.Target += delta;
	}

	if (MathF.Abs(Raylib.GetMouseWheelMove()) > 0)
	{
		// Set the target to match, so that the camera maps the world space point 
		// under the cursor to the screen space point under the cursor at any zoom.
		Vector2 mouseWorldPos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
		camera.Offset = Raylib.GetMousePosition();
		camera.Target = mouseWorldPos;

		UpdateCameraZoom();
	}

	if (Raylib.IsKeyPressed(KeyboardKey.KEY_C))
	{
		ResetCamera();
	}
}

void ResetCamera()
{
	if (spriteAtlas != null && spriteAtlas.Width != 0 && spriteAtlas.Height != 0)
	{
		// When zooming the camera to fit the texture into the viewport, account for the menubar and some extra space.
		const int menuBarHeight = 19;
		const int sizeOffset = menuBarHeight + 41;

		float zoomWidth = Raylib.GetRenderWidth() / (float)spriteAtlas.Width;
		float zoomHeight = (Raylib.GetRenderHeight() - sizeOffset) / (float)spriteAtlas.Height;
		camera.Zoom = MathF.Min(zoomWidth, zoomHeight);
		camera.Target = new Vector2(spriteAtlas.Width / 2f, spriteAtlas.Height / 2f);
		camera.Offset = new Vector2(
			(Raylib.GetRenderWidth() / 2f),
			(Raylib.GetRenderHeight() - sizeOffset) / 2f + sizeOffset / 2f);
	}
	else
	{
		camera.Zoom = 1f;
		camera.Target = Vector2.Zero;
		camera.Offset = Vector2.Zero;
	}
}

void Shutdown()
{
	rlImGui.Shutdown();
	Raylib.CloseWindow();
}

void UpdateCameraZoom()
{
	const float zoomSpeed = 0.125f;
	float zoomFactor = (float)Math.Log(camera.Zoom + 1, 10) * zoomSpeed;
	camera.Zoom = Math.Clamp(camera.Zoom + Raylib.GetMouseWheelMove() * zoomFactor, cameraMinZoom, cameraMaxZoom);
}

Texture2D CreateCheckerTexture()
{
	const int contrast = 40;
	int highValue = Math.Clamp(checkerBaseValue + contrast, 0, 255);
	int lowValue = Math.Clamp(checkerBaseValue - contrast, 0, 255);
	Image checkerImage = Raylib.GenImageChecked(
		spriteAtlas.Texture.Width, spriteAtlas.Texture.Height, 1, 1,
		new Color(lowValue, lowValue, lowValue, 255),
		new Color(highValue, highValue, highValue, 255));
	Texture2D texture = Raylib.LoadTextureFromImage(checkerImage);
	Raylib.UnloadImage(checkerImage);
	return texture;
}