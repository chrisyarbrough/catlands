namespace CatLands.Editor;

using System.Numerics;
using System.Text;
using ImGuiNET;
using NativeFileDialogSharp;
using Raylib_cs;

public class TileBrushWindow : Window
{
	private readonly MapTextures mapTextures = new();
	private int layerIndex = -1;
	private int tileId = -1;
	private Coord? selectedCoord;

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
		layerIndex = -1;
		tileId = -1;
	}

	public override void OnSceneGui()
	{
		if (Map.Current == null || layerIndex == -1 || tileId == -1)
			return;

		if (SceneView.Current != null && SceneView.Current.IsMouseOverWindow)
		{
			Vector2 mouseWorldPosition = SceneView.Current.GetMouseWorldPosition();
			Coord gridPosition = Grid.WorldToCoord(mouseWorldPosition);

			DrawOutline(gridPosition, Grid.TileRenderSize, 2);

			if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
			{
				if (selectedCoord != gridPosition)
				{
					selectedCoord = gridPosition;
					CommandManager.Execute(new MapEditCommand(Map.Current, layerIndex, gridPosition, tileId));
				}
			}
		}
	}

	private static void DrawOutline(Coord gridPosition, int rectSize, float lineThickness)
	{
		Coord screenPos = Grid.CoordToWorld(gridPosition);
		var outline = new Rectangle(screenPos.X, screenPos.Y, rectSize, rectSize);
		Raylib.DrawRectangleLinesEx(outline, lineThickness, Color.RED);
	}

	protected override void DrawContent()
	{
		if (Map.Current == null)
			return;

		DrawLayerDropdown();

		mapTextures.UpdateLoadState();

		if (mapTextures.TextureCount <= layerIndex || layerIndex == -1)
			return;

		string textureId = Map.Current.GetLayer(layerIndex).TexturePath;
		Texture2D tileset = mapTextures.Get(textureId);

		DrawTileDropdown(tileset);

		ImGui.NewLine();
		ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(1, 1));
		ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));

		int xTileCount = tileset.Width / Grid.TileSourceSize;
		int yTileCount = tileset.Height / Grid.TileSourceSize;
		for (int y = 0; y < yTileCount; y++)
		{
			for (int x = 0; x < xTileCount; x++)
			{
				ImGui.SameLine();

				int tileIndex = x + y * xTileCount;
				float w = Grid.TileSourceSize / (float)tileset.Width;
				float h = Grid.TileSourceSize / (float)tileset.Height;

				if (ImGui.ImageButton(
					    tileIndex.ToString(),
					    new IntPtr(tileset.Id),
					    new Vector2(32, 32),
					    new Vector2(x * w, y * h),
					    new Vector2(x * w + w, y * h + h)))
				{
					this.tileId = tileIndex;
					Prefs.Set("TileBrushWindow.TileId", tileId);
				}

				if (tileIndex == this.tileId)
				{
					ImGui.GetWindowDrawList().AddRect(
						ImGui.GetItemRectMin(),
						ImGui.GetItemRectMax(),
						ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 0.0f, 0.0f, 1.0f)),
						rounding: 0,
						ImDrawFlags.None,
						thickness: 2);
				}
			}
			ImGui.NewLine();
		}

		ImGui.PopStyleVar();
		ImGui.PopStyleVar();
	}

	private void DrawTileDropdown(Texture2D tileset)
	{
		var sb = new StringBuilder();
		int xTileCount = tileset.Width / Grid.TileSourceSize;
		int yTileCount = tileset.Height / Grid.TileSourceSize;

		for (int y = 0; y < yTileCount; y++)
		{
			for (int x = 0; x < xTileCount; x++)
			{
				int tileIndex = x + y * xTileCount;
				sb.Append(tileIndex).Append('\0');
			}
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
				CommandManager.Execute(new AddLayerCommand(Map.Current, result.Path));
				if (layerIndex == -1)
					layerIndex = 0;
			}
		}
	}
}