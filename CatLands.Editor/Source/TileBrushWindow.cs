namespace CatLands.Editor;

using System.Numerics;
using System.Text;
using ImGuiNET;
using NativeFileDialogSharp;
using Raylib_cs;
using SpriteEditor;

public class TileBrushWindow : Window
{
	private int layerIndex = -1;
	private int tileId = -1;
	private Coord? selectedCoord;
	private bool keepOriginalLayout = true;
	private bool wasMouseOverSceneViewLastFrame;

	public TileBrushWindow() : base("Tile Brush")
	{
	}

	public override void Setup()
	{
		layerIndex = Prefs.Get("TileBrushWindow.LayerIndex", defaultValue: layerIndex);
		tileId = Prefs.Get("TileBrushWindow.TileId", defaultValue: tileId);
		Map.CurrentChanged += OnCurrentChanged;
	}

	public override void Shutdown()
	{
		Map.CurrentChanged -= OnCurrentChanged;
	}

	private void OnCurrentChanged()
	{
		if (Map.Current != null && Map.Current.LayerCount > 0)
			layerIndex = 0;
		else
			layerIndex = -1;

		if (Map.Current != null && Map.Current.LayerCount > 0 && Map.Current.Layers.First().Tiles.Any())
			tileId = 0;
		else
			tileId = -1;
	}

	public override void Update()
	{
		if (SceneView.Current == null || Map.Current == null || Map.Current.LayerCount == 0)
			return;

		bool mouseOverWindow = SceneView.Current.IsMouseOverWindow;

		if (Raylib.GetMouseDelta().LengthSquared() > 0f && mouseOverWindow)
		{
			SceneView.RepaintAll();
		}

		if (mouseOverWindow == false && wasMouseOverSceneViewLastFrame)
		{
			// When leaving the window, remove the preview brush.
			MapDisplay.RemovePreview();
			SceneView.RepaintAll();
		}

		wasMouseOverSceneViewLastFrame = mouseOverWindow;

		if (mouseOverWindow && Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
		{
			Vector2 mouseWorldPosition = SceneView.Current.GetMouseWorldPosition();
			Coord gridCoord = Grid.WorldToCoord(mouseWorldPosition);

			if (selectedCoord != gridCoord)
			{
				selectedCoord = gridCoord;
				CommandManager.Execute(new MapEditCommand(Map.Current, layerIndex, gridCoord, tileId));
				SceneView.RepaintAll();
			}
		}
	}

	public override void OnSceneGui()
	{
		if (Map.Current == null || layerIndex == -1 || tileId == -1 || Map.Current.LayerCount == 0)
			return;

		layerIndex = Math.Clamp(layerIndex, 0, Map.Current.LayerCount - 1);

		if (SceneView.Current != null && SceneView.Current.IsMouseOverWindow)
		{
			Vector2 mouseWorldPosition = SceneView.Current.GetMouseWorldPosition();
			Coord gridCoord = Grid.WorldToCoord(mouseWorldPosition);

			float scaleFactor = 1.0f / SceneView.Current.CameraZoom;
			float lineWidth = 2 * scaleFactor;

			DrawBrushIndicator(gridCoord, Grid.TileRenderSize, lineWidth);
		}
	}

	private void DrawBrushIndicator(Coord gridCoord, int rectSize, float lineThickness)
	{
		MapDisplay.AddPreview(new TileRenderInfo(layerIndex, gridCoord, tileId));

		// Outline
		Coord screenPos = Grid.CoordToWorld(gridCoord);
		var rect = new Rectangle(screenPos.X, screenPos.Y, rectSize, rectSize);
		Raylib.DrawRectangleLinesEx(rect, lineThickness, Color.RED);
	}

	protected override void DrawContent()
	{
		if (Map.Current == null)
			return;

		DrawLayerDropdown();

		MapTextures.UpdateLoadState();

		if (MapTextures.TextureCount <= layerIndex || layerIndex == -1)
			return;

		string textureId = Map.Current.GetLayer(layerIndex).TexturePath;
		SpriteAtlas atlas = MapTextures.GetAtlas(textureId);

		DrawTileDropdown(atlas);
		ImGui.Checkbox("Original Layout", ref keepOriginalLayout);
		ImGui.NewLine();

		ImGui.BeginChild("Tileset");
		DrawTileSelector(atlas, textureId);
		ImGui.EndChild();
	}

	private void DrawTileSelector(SpriteAtlas atlas, string textureId)
	{
		Texture2D tileset = MapTextures.GetTexture(textureId);
		var tileSetPointer = new IntPtr(tileset.Id);

		ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(1, 1));
		ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));


		float rightEdge = ImGui.GetWindowPos().X + ImGui.GetContentRegionMax().X;
		Vector2 selectedRectMin = default;
		Vector2 selectedRectMax = default;

		Vector2 offset = ImGui.GetCursorPos();
		const int upscale = 2;

		for (int i = 0; i < atlas.SpriteRects.Count; i++)
		{
			Rectangle rect = atlas.SpriteRects[i];

			if (keepOriginalLayout)
				ImGui.SetCursorPos(offset + new Vector2(rect.X, rect.Y) * upscale);

			ImGui.Image(
				tileSetPointer,
				new Vector2(rect.Width, rect.Height) * upscale,
				new Vector2(rect.X / tileset.Width, rect.Y / tileset.Height),
				new Vector2(rect.xMax() / tileset.Width, rect.yMax() / tileset.Height));

			// Drawing an image and then checking for the click is much faster than using the ImageButton.
			if (ImGui.IsItemClicked())
			{
				this.tileId = i;
				Prefs.Set("TileBrushWindow.TileId", tileId);
			}

			if (keepOriginalLayout == false)
			{
				float itemRightEdge = ImGui.GetItemRectMax().X;
				if (rightEdge - itemRightEdge >= 32)
					ImGui.SameLine();
			}

			if (i == tileId)
			{
				selectedRectMin = ImGui.GetItemRectMin();
				selectedRectMax = ImGui.GetItemRectMax();
			}

			if (ImGui.IsItemHovered())
			{
				DrawRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), new Vector4(0.9f, 0.1f, 0.1f, 0.35f));
			}
		}

		DrawRect(selectedRectMin, selectedRectMax, new Vector4(1f, 0f, 0f, 1f));


		ImGui.PopStyleVar();
		ImGui.PopStyleVar();
	}

	private static void DrawRect(Vector2 min, Vector2 max, Vector4 color)
	{
		ImGui.GetWindowDrawList().AddRect(
			min + Vector2.One * 0.5f,
			max - Vector2.One * 0.5f,
			ImGui.ColorConvertFloat4ToU32(color),
			rounding: 0,
			ImDrawFlags.None,
			thickness: 2);
	}

	private void DrawTileDropdown(SpriteAtlas atlas)
	{
		var sb = new StringBuilder();

		for (int i = 0; i < atlas.SpriteRects.Count; i++)
		{
			sb.Append(i).Append('\0');
		}

		if (ImGui.Combo("Tile", ref tileId, sb.ToString()))
			Prefs.Set("TileBrushWindow.TileId", tileId);
	}

	private void DrawLayerDropdown()
	{
		string[] options = Map.Current!.Layers.Select(x => x.Name).ToArray();
		if (ImGui.Combo("Layer", ref layerIndex, options, options.Length))
		{
			tileId = 0;
			Prefs.Set("TileBrushWindow.LayerIndex", layerIndex);
		}

		ImGui.SameLine();

		if (ImGui.Button("Add"))
		{
			DialogResult result = Dialog.FileOpen(defaultPath: "Assets");
			if (result.IsOk)
			{
				var command = new AddLayerCommand(Map.Current, result.Path);
				CommandManager.Execute(command);
				layerIndex = command.AddedLayerId;
				tileId = 0;
			}
		}
	}
}