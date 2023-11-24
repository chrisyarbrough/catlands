using System.Numerics;
using CatLands.SpriteEditor;
using ImGuiNET;
using NativeFileDialogSharp;
using Raylib_cs;
using rlImGui_cs;

string textureFilePath = string.Empty;
Texture2D texture = default;
var camera = new Camera2D(Vector2.Zero, Vector2.Zero, 0f, 1f);
Rectangle gizmoRect = new Rectangle(0, 0, 16, 16);

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
	Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE | ConfigFlags.FLAG_VSYNC_HINT);
	Raylib.InitWindow(width: 1280, height: 800, title: "CatLands SpriteEditor");
	Raylib.SetTargetFPS(120);

	rlImGui.Setup(darkTheme: true, enableDocking: true);
	ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.125f, 0.125f, 0.125f, 1f));

	ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;

	if (File.Exists(textureFilePath))
	{
		texture = Raylib.LoadTexture(textureFilePath);
	}

	RecenterCamera();
}

void Update()
{
	ImGui.BeginMainMenuBar();
	if (ImGui.BeginMenu("File"))
	{
		if (ImGui.MenuItem("Open"))
		{
			DialogResult result = Dialog.FileOpen();
			if (result.IsOk)
			{
				textureFilePath = result.Path;
				File.WriteAllText(configFilePath, textureFilePath);
				texture = Raylib.LoadTexture(textureFilePath);
			}
		}

		if (ImGui.MenuItem("Save"))
		{
		}

		ImGui.EndMenu();
	}

	if (ImGui.MenuItem("Recenter"))
		RecenterCamera();

	ImGui.EndMainMenuBar();

	Raylib.BeginMode2D(camera);
	UpdateCamera();
	Vector2 mousePos = Raylib.GetMousePosition();

	if (texture.Id != 0)
	{
		Raylib.DrawTexture(texture, 0, 0, Color.WHITE);
		gizmoRect = RectangleGizmo.Draw(gizmoRect, mousePos, camera);
		ImGui.LabelText("Rect", $"{gizmoRect.X} {gizmoRect.Y} {gizmoRect.Width} {gizmoRect.Height}");
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

		const float zoomSpeed = 0.125f;
		float zoomFactor = (float)Math.Log(camera.Zoom + 1, 10) * zoomSpeed;
		camera.Zoom = Math.Clamp(camera.Zoom + Raylib.GetMouseWheelMove() * zoomFactor, 0.1f, 20f);
	}

	if (Raylib.IsKeyPressed(KeyboardKey.KEY_C))
	{
		RecenterCamera();
	}
}

void RecenterCamera()
{
	camera.Target = Vector2.Zero;
	var offset = new Vector2(Raylib.GetRenderWidth() / 2f, Raylib.GetRenderHeight() / 2f);

	if (texture.Width != 0 && texture.Height != 0)
	{
		offset.X -= texture.Width / 2f;
		offset.Y -= texture.Height / 2f;
	}

	camera.Offset = offset;
	camera.Zoom = 1f;
}

void Shutdown()
{
	rlImGui.Shutdown();
	Raylib.CloseWindow();
}