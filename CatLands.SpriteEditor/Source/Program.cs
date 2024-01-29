namespace CatLands.SpriteEditor;

using System.Numerics;
using CatLands;
using Editor;
using ImGuiNET;
using NativeFileDialogSharp;
using Raylib_cs;

internal class Program : App
{
	const string configFilePath = "LastFilePath.txt";

	string textureFilePath = string.Empty;
	SpriteAtlas? spriteAtlas;
	CameraController camera;
	Slicer slicer;
	CheckerBackground checkerBackground = new();

	SpriteAtlasViewModel viewModel;
	bool showDemoWindow = false;
	static AppWindow window;

	public static void Main(string[] args)
	{
		Directory.SetCurrentDirectory(AppContext.BaseDirectory);

		Program program = new Program()
		{
			BaseTitle = "CatLands Sprite Editor",
		};
		window = new AppWindow(program);
		program.viewModel = new SpriteAtlasViewModel(window);
		window.Run();
	}

	private Vector2 GetTextureSize() => spriteAtlas?.TextureSize ?? Vector2.Zero;

	public override void Initialize()
	{
		camera = new SpriteEditorCamera(GetTextureSize);

		if (CommandLineArgs.Length > 1)
		{
			textureFilePath = CommandLineArgs[1];
		}
		else if (File.Exists(configFilePath))
		{
			textureFilePath = File.ReadAllText(configFilePath);
		}


		if (!File.Exists(ImGui.GetIO().IniFilename))
			ImGui.LoadIniSettingsFromDisk("imgui_animation.ini");

		if (File.Exists(textureFilePath))
		{
			LoadSpriteAtlas();
		}

		SceneSettingsWindow.Add("Selection", DrawSelectedSpriteGui);


		MainWindow.InitializeLayout(new List<Func<Window>>
		{
			CreateAnimationWindow<AnimationSelectorWindow>,
			CreateAnimationWindow<AnimationFramesWindow>,
			CreateAnimationWindow<AnimationPreviewWindow>,
			CreateAnimationWindow<AnimationTimelineWindow>,
			() => new SceneSettingsWindow(),
		});
	}

	private T CreateAnimationWindow<T>() where T : Window
	{
		return (T)Activator.CreateInstance(typeof(T), args: new[] { viewModel });
	}

	public override void Update()
	{
		if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SUPER) && Raylib.IsKeyPressed(KeyboardKey.KEY_S))
			Save();

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
				Save();
			}

			ImGui.EndMenu();
		}

		if (ImGui.MenuItem("Slice", enabled: spriteAtlas != null))
		{
			ImGui.OpenPopup("SlicePopup");
		}

		if (ImGui.BeginPopup("SlicePopup") && spriteAtlas != null)
		{
			slicer.DrawGui();
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
			DrawRectangleGizmos();
		}

		camera.End();
	}

	private void DrawRectangleGizmos()
	{
		DrawSpriteAtlas();
		MainWindow.DrawWindows();

		if (RectangleGizmo.DrawGizmos)
		{
			if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
				viewModel.RecordSnapshot();

			Gizmos.Draw(camera, viewModel);

			if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
				viewModel.EvaluateDirty();
		}

		if (KeyboardKeyAction.Delete.IsStarted() && TileSelection.HasSelection())
		{
			viewModel.DeleteTiles(TileSelection.GetSelection().OrderByDescending(x => x));
			TileSelection.ClearSelection();
		}

		if (KeyboardKeyAction.Undo.IsStarted())
		{
			viewModel.Undo();
		}
		else if (KeyboardKeyAction.Redo.IsStarted())
		{
			viewModel.Redo();
		}

		// Merge selected rects.
		if (Raylib.IsKeyPressed(KeyboardKey.KEY_M) && TileSelection.HasSelection() &&
		    !ImGui.GetIO().WantCaptureKeyboard)
		{
			int newId = viewModel.MergeTiles(TileSelection.GetSelection());
			TileSelection.SetSingleSelection(newId);
		}
	}

	private void DrawSpriteAtlas()
	{
		var position = new Vector2(0, 0);
		checkerBackground.DrawScene(position);
		Raylib.DrawTextureV(spriteAtlas.Texture, position, Color.WHITE);
	}

	void DrawSelectedSpriteGui()
	{
		ImGui.TextUnformatted(
			$"Hovered tile: {(Gizmos.HoveredControl != null ? Gizmos.HoveredControl.ToString() : string.Empty)}");

		if (TileSelection.HasSelection() && spriteAtlas != null)
		{
			foreach (int index in TileSelection.GetSelection())
			{
				if (viewModel.TryGetRect(index, out Rect rect))
					ImGui.TextUnformatted($"{index} x:{rect.X} y:{rect.Y} w:{rect.Width} h:{rect.Height}");
			}
		}
	}

	void LoadSpriteAtlas()
	{
		spriteAtlas = SpriteAtlas.Load(textureFilePath);
		slicer = new(viewModel);

		viewModel.SetTarget(spriteAtlas);
		checkerBackground.Refresh(spriteAtlas.TextureSize);
		camera.Reset();
		window.UpdateSubTitle(Path.GetFileName(textureFilePath));
	}

	private void Save()
	{
		viewModel.Save();
	}
}