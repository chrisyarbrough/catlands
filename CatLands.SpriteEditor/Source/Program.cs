namespace CatLands.SpriteEditor;

using System.Numerics;
using CatLands;
using Editor;
using ImGuiNET;
using NativeFileDialogSharp;
using Raylib_cs;
using rlImGui_cs;

internal class Program
{
	public const string Title = "CatLands SpriteEditor";

	public static void Main(string[] args)
	{
		Directory.SetCurrentDirectory(AppContext.BaseDirectory);

		string textureFilePath = string.Empty;
		SpriteAtlas? spriteAtlas = default;
		var camera = new SpriteEditorCamera(GetTextureSize);
		Slicer slicer = new();
		CheckerBackground checkerBackground = new();
		List<int> hoveredRects = new();
		int hoveredControl = -1;

		bool showDemoWindow = false;

		Vector2 GetTextureSize()
		{
			return spriteAtlas?.TextureSize ?? Vector2.Zero;
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

		AnimationEditor animationEditor = new();

		Initialize();

		MainWindow.InitializeLayout(new List<Func<Window>>
		{
			() => animationEditor.CreateWindow<AnimationSelectorWindow>(),
			() => animationEditor.CreateWindow<AnimationFramesWindow>(),
			() => animationEditor.CreateWindow<AnimationPreviewWindow>(),
			() => animationEditor.CreateWindow<AnimationTimelineWindow>(),
			() => new SceneSettingsWindow()
		});

		if (!File.Exists(ImGui.GetIO().IniFilename))
			ImGui.LoadIniSettingsFromDisk("imgui_animation.ini");

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
			Raylib.InitWindow(width: 1280, height: 800, Title);
			Raylib.SetTargetFPS(120);

			rlImGui.Setup(darkTheme: true, enableDocking: true);
			ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.125f, 0.125f, 0.125f, 1f));

			// Bug: the rounding causes a 1 pixel transparent gab between window title and content.
			//ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 4f);

			ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;

			if (File.Exists(textureFilePath))
			{
				LoadSpriteAtlas();
			}

			SceneSettingsWindow.Add("Selection", DrawSelectedSpriteGui);
		}

		void DrawSelectedSpriteGui()
		{
			ImGui.TextUnformatted($"Hovered tile: {(hoveredControl != -1 ? hoveredControl.ToString() : string.Empty)}");

			if (TileSelection.HasSelection() && spriteAtlas != null)
			{
				foreach (int index in TileSelection.GetSelection())
				{
					Rectangle rect = spriteAtlas.SpriteRects[index];
					ImGui.TextUnformatted($"{index} x:{rect.X} y:{rect.Y} w:{rect.Width} h:{rect.Height}");
				}
			}
		}

		void LoadSpriteAtlas()
		{
			spriteAtlas = SpriteAtlas.Load(textureFilePath);
			checkerBackground.Refresh(spriteAtlas.TextureSize);
			camera.Reset();
			SaveDirtyTracker.MarkClean(spriteAtlas!);
			animationEditor.SetSpriteAtlas(spriteAtlas);
		}

		void Update()
		{
			if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SUPER) && Raylib.IsKeyPressed(KeyboardKey.KEY_S))
				Save(spriteAtlas);

			MainWindow.Update();

			Cursor.Reset();
			ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
			DrawMenu();
			DrawScene();
			ImGui.End(); // DockArea
			Cursor.Draw();

			if (showDemoWindow)
				ImGui.ShowDemoWindow();
		}

		void DrawMenu()
		{
			ImGui.BeginMainMenuBar();
			if (ImGui.BeginMenu("File"))
			{
				if (ImGui.MenuItem("Open"))
				{
					// Use last opened directory.
					string? path = File.Exists(textureFilePath)
						? Path.GetDirectoryName(textureFilePath)
						: Directory.GetCurrentDirectory();

					DialogResult result = Dialog.FileOpen(defaultPath: path);
					if (result.IsOk)
					{
						textureFilePath = result.Path;
						File.WriteAllText(configFilePath, textureFilePath);
						LoadSpriteAtlas();
					}
				}

				if (ImGui.MenuItem("Save", enabled: spriteAtlas != null))
				{
					Save(spriteAtlas);
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

			if (ImGui.BeginMenu("Window"))
			{
				if (ImGui.MenuItem("Save Layout"))
				{
					ImGui.SaveIniSettingsToDisk("MyLayout");
				}

				ImGui.Separator();
				ImGui.EndMenu();
			}

			ImGui.EndMainMenuBar();
		}

		void DrawScene()
		{
			camera.Begin();
			camera.Update(canBeginInputAction: !ImGui.GetIO().WantCaptureMouse);

			if (spriteAtlas != null)
			{
				var position = new Vector2(0, 0);

				checkerBackground.DrawScene(position);
				Raylib.DrawTextureV(spriteAtlas.Texture, position, Color.WHITE);

				MainWindow.Draw();

				if (RectangleGizmo.DrawGizmos)
				{
					Rectangle worldBounds = camera.GetWorldBounds();
					Vector2 mouseWorldPos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera.State);

					hoveredControl = -1;
					hoveredRects.Clear();

					if (GuiUtility.HotControl == -1)
					{
						// Draw hovered controls first to capture input.
						for (int i = 0; i < spriteAtlas.SpriteRects.Count; i++)
						{
							if (Raylib.CheckCollisionRecs(spriteAtlas.SpriteRects[i], worldBounds))
							{
								if (spriteAtlas.SpriteRects[i].IsPointWithin(mouseWorldPos))
								{
									hoveredRects.Add(i);
								}
							}
						}

						// Chose the rect with the smallest area.
						if (hoveredRects.Count > 0 && !ImGui.GetIO().WantCaptureMouse)
						{
							hoveredControl = hoveredRects.OrderBy(i => spriteAtlas.SpriteRects[i].Area()).First();
							spriteAtlas.SpriteRects[hoveredControl] = RectangleGizmo.Draw(
								spriteAtlas.SpriteRects[hoveredControl], hoveredControl, mouseWorldPos, camera.State,
								spriteAtlas,
								UpdatePhase.Input, isHovered: true);
						}
					}

					if (hoveredControl == -1 && GuiUtility.HotControl >= 0 &&
					    GuiUtility.HotControl < spriteAtlas.SpriteRects.Count)
					{
						spriteAtlas.SpriteRects[GuiUtility.HotControl] = RectangleGizmo.Draw(
							spriteAtlas.SpriteRects[GuiUtility.HotControl], GuiUtility.HotControl, mouseWorldPos,
							camera.State,
							spriteAtlas, UpdatePhase.Input, isHovered: true);
					}

					// Draw default rects
					for (int i = 0; i < spriteAtlas.SpriteRects.Count; i++)
					{
						if (Raylib.CheckCollisionRecs(spriteAtlas.SpriteRects[i], worldBounds))
						{
							if (i != hoveredControl && !TileSelection.IsSelected(i))
							{
								spriteAtlas.SpriteRects[i] = RectangleGizmo.Draw(
									spriteAtlas.SpriteRects[i], i, mouseWorldPos, camera.State, spriteAtlas,
									UpdatePhase.Draw,
									isHovered: false);
							}
						}
					}

					// Draw selection
					for (int i = 0; i < spriteAtlas.SpriteRects.Count; i++)
					{
						if (Raylib.CheckCollisionRecs(spriteAtlas.SpriteRects[i], worldBounds))
						{
							if (TileSelection.IsSelected(i))
							{
								spriteAtlas.SpriteRects[i] = RectangleGizmo.Draw(
									spriteAtlas.SpriteRects[i], i, mouseWorldPos, camera.State, spriteAtlas,
									UpdatePhase.Draw,
									isHovered: false);
							}
						}
					}

					// Draw hovered control
					if (hoveredControl != -1)
					{
						spriteAtlas.SpriteRects[hoveredControl] = RectangleGizmo.Draw(
							spriteAtlas.SpriteRects[hoveredControl], hoveredControl, mouseWorldPos, camera.State,
							spriteAtlas,
							UpdatePhase.Draw, isHovered: true);
					}

					// Draw hot control
					if (GuiUtility.HotControl >= 0 && GuiUtility.HotControl < spriteAtlas.SpriteRects.Count)
					{
						spriteAtlas.SpriteRects[GuiUtility.HotControl] = RectangleGizmo.Draw(
							spriteAtlas.SpriteRects[GuiUtility.HotControl], GuiUtility.HotControl, mouseWorldPos,
							camera.State,
							spriteAtlas, UpdatePhase.Draw, isHovered: true);
					}

					BoxSelect.Draw(camera.State, controlId: spriteAtlas.SpriteRects.Count, rectangle =>
					{
						TileSelection.ClearSelection();
						for (int i = 0; i < spriteAtlas.SpriteRects.Count; i++)
						{
							if (rectangle.Encloses(spriteAtlas.SpriteRects[i]))
							{
								TileSelection.AddToSelection(i);
							}
						}
					});
				}

				if ((Raylib.IsKeyPressed(KeyboardKey.KEY_DELETE) || Raylib.IsKeyPressed(KeyboardKey.KEY_BACKSPACE)) &&
				    TileSelection.HasSelection())
				{
					UndoManager.RecordSnapshot(spriteAtlas);

					// TODO: Removing rects will cause all tile IDs to shift and break existing maps.
					// Probably better to use more stable ids for the tiles.
					foreach (int i in TileSelection.GetSelection().OrderByDescending(x => x))
						spriteAtlas.SpriteRects.RemoveAt(i);

					TileSelection.ClearSelection();
					SaveDirtyTracker.EvaluateDirty(spriteAtlas);
				}

				if (Raylib.IsKeyPressed(KeyboardKey.KEY_Z) &&
				    (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SUPER) || Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL)))
				{
					UndoManager.Undo(spriteAtlas);
				}
				else if (Raylib.IsKeyPressed(KeyboardKey.KEY_Y) &&
				         (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SUPER) ||
				          Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL)))
				{
					UndoManager.Redo(spriteAtlas);
				}

				// Merge selected rects.
				if (Raylib.IsKeyPressed(KeyboardKey.KEY_M) && TileSelection.HasSelection() &&
				    !ImGui.GetIO().WantCaptureKeyboard)
				{
					Rectangle mergedRect = spriteAtlas.SpriteRects[TileSelection.GetSelection().First()];

					foreach (int i in TileSelection.GetSelection())
					{
						Rectangle spriteRect = spriteAtlas.SpriteRects[i];

						// Expand the mergedRect to include the current spriteRect
						float minX = Math.Min(mergedRect.X, spriteRect.X);
						float minY = Math.Min(mergedRect.Y, spriteRect.Y);
						float maxX = Math.Max(mergedRect.X + mergedRect.Width, spriteRect.X + spriteRect.Width);
						float maxY = Math.Max(mergedRect.Y + mergedRect.Height, spriteRect.Y + spriteRect.Height);

						mergedRect.X = minX;
						mergedRect.Y = minY;
						mergedRect.Width = maxX - minX;
						mergedRect.Height = maxY - minY;
					}

					foreach (int i in TileSelection.GetSelection().OrderByDescending(x => x))
						spriteAtlas.SpriteRects.RemoveAt(i);

					int id = spriteAtlas.Add(mergedRect);
					TileSelection.SetSingleSelection(id);
				}
			}

			camera.End();
		}

		void Shutdown()
		{
			rlImGui.Shutdown();
			Raylib.CloseWindow();
		}
	}

	private static void Save(SpriteAtlas? spriteAtlas)
	{
		if (spriteAtlas != null)
		{
			spriteAtlas.Save();
			SaveDirtyTracker.MarkClean(spriteAtlas);
		}
	}
}