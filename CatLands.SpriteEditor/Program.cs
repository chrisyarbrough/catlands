using System.Numerics;
using CatLands;
using CatLands.SpriteEditor;
using ImGuiNET;
using NativeFileDialogSharp;
using Raylib_cs;
using rlImGui_cs;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

string textureFilePath = string.Empty;
SpriteAtlas? spriteAtlas = default;
var camera = new SpriteEditorCamera(GetTextureSize);

Slicer slicer = new();
CheckerBackground checkerBackground = new();


Vector2 GetTextureSize()
{
	return spriteAtlas != null ? spriteAtlas.TextureSize : Vector2.Zero;
}

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
	Raylib.SetTraceLogLevel(TraceLogLevel.LOG_WARNING);
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
		LoadSpriteAtlas();
	}

	SettingsWindow.Add("Selected Sprite", DrawSelectedSpriteGui);
}

void DrawSelectedSpriteGui()
{
	if (Selection.HasSelection() && spriteAtlas != null)
	{
		Rectangle rect = spriteAtlas.SpriteRects[Selection.GetSingleSelection()];
		ImGui.TextUnformatted($"x:{rect.X} y:{rect.Y} w:{rect.Width} h:{rect.Height}");
	}
}

void LoadSpriteAtlas()
{
	spriteAtlas = SpriteAtlas.Load(textureFilePath, TextureFilter.TEXTURE_FILTER_POINT);
	checkerBackground.Refresh(spriteAtlas.TextureSize);
	camera.Reset();
}

void Update()
{
	GuiUtility.ResetControlId();
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
				LoadSpriteAtlas();
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

	if (ImGui.BeginPopup("SlicePopup") && spriteAtlas != null)
	{
		slicer.DrawGui(spriteAtlas);
		ImGui.EndPopup();
	}

	ImGui.EndMainMenuBar();
}

void DrawScene()
{
	Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
	camera.Begin();

	if (spriteAtlas != null)
	{
		var position = new Vector2(0, 0);

		checkerBackground.DrawScene(position);
		Raylib.DrawTextureV(spriteAtlas.Texture, position, Color.WHITE);


		SettingsWindow.Draw();

		if (RectangleGizmo.DrawGizmos)
		{
			Rectangle worldBounds = camera.GetWorldBounds();
			Vector2 mouseWorldPos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera.State);

			int hoveredControl = -1;

			// Draw hovered controls first to capture input.
			for (int i = 0; i < spriteAtlas.SpriteRects.Count; i++)
			{
				if (Raylib.CheckCollisionRecs(spriteAtlas.SpriteRects[i], worldBounds))
				{
					if (spriteAtlas.SpriteRects[i].IsPointWithin(mouseWorldPos))
					{
						spriteAtlas.SpriteRects[i] = RectangleGizmo.Draw(
							spriteAtlas.SpriteRects[i], i, mouseWorldPos, camera.State, spriteAtlas);

						hoveredControl = i;
						break;
					}
				}
			}

			for (int i = 0; i < spriteAtlas.SpriteRects.Count; i++)
			{
				if (Raylib.CheckCollisionRecs(spriteAtlas.SpriteRects[i], worldBounds))
				{
					if (i != hoveredControl)
					{
						spriteAtlas.SpriteRects[i] = RectangleGizmo.Draw(
							spriteAtlas.SpriteRects[i], i, mouseWorldPos, camera.State, spriteAtlas);
					}
				}
			}

			BoxSelection.Draw(camera.State, rectangle =>
			{
				Selection.ClearSelection();
				for (int i = 0; i < spriteAtlas.SpriteRects.Count; i++)
				{
					if (rectangle.IsPointWithin(spriteAtlas.SpriteRects[i].Center()))
					{
						Selection.AddToSelection(i);
					}
				}
			});
		}

		if ((Raylib.IsKeyPressed(KeyboardKey.KEY_DELETE) || Raylib.IsKeyPressed(KeyboardKey.KEY_BACKSPACE)) &&
		    Selection.HasSelection())
		{
			UndoManager.RecordSnapshot(spriteAtlas);

			foreach (int i in Selection.GetSelection().OrderByDescending(x => x))
				spriteAtlas.SpriteRects.RemoveAt(i);

			Selection.ClearSelection();
		}

		if (Raylib.IsKeyPressed(KeyboardKey.KEY_Z) &&
		    (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SUPER) || Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL)))
		{
			UndoManager.Undo(spriteAtlas);
		}
		else if (Raylib.IsKeyPressed(KeyboardKey.KEY_Y) &&
		         (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SUPER) || Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL)))
		{
			UndoManager.Redo(spriteAtlas);
		}
	}

	camera.End();
}

void Shutdown()
{
	rlImGui.Shutdown();
	Raylib.CloseWindow();
}