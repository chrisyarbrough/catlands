namespace CatLands.Editor;

using System.Numerics;
using System.Text;
using ImGuiNET;
using Raylib_cs;

public class SingleTileBrush : Brush
{
	public override string DisplayName => "Single";

	private readonly DirectionalInputAction directionalInputAction = new();

	private bool keepOriginalLayout = true;
	private Dictionary<int, Rect>? screenRects;
	private int tileId = -1;
	private Coord? hoveredGridCoord;
	private Coord? drawnGridCoord;
	private bool wasMouseOverSceneViewLastFrame;

	public override void Initialize()
	{
		if (ValidMapWithLayersAndTiles())
			tileId = Prefs.Get("TileBrushWindow.TileId", defaultValue: tileId);
	}

	public override void OnLayersChanged()
	{
		if (ValidMapWithLayersAndTiles())
			tileId = 0;
		else
			tileId = -1;
	}

	private static bool ValidMapWithLayersAndTiles()
	{
		return Map.Current != null && Map.Current.LayerCount > 0 && Map.Current.Layers.First().Tiles.Any();
	}

	public override void Update(SpriteAtlas atlas, int layerIndex, bool mouseOverWindow)
	{
		if (mouseOverWindow == false && wasMouseOverSceneViewLastFrame)
		{
			// When leaving the window, remove the preview brush.
			hoveredGridCoord = null;
			MapDisplay.ClearPreviews();
			SceneView.RepaintAll();
		}

		wasMouseOverSceneViewLastFrame = mouseOverWindow;

		if (mouseOverWindow)
		{
			Vector2 mouseWorldPosition = SceneView.Current!.GetMouseWorldPosition();
			Coord gridCoord = Grid.WorldToCoord(mouseWorldPosition);

			if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
			{
				if (drawnGridCoord != gridCoord)
				{
					drawnGridCoord = gridCoord;
					CommandManager.Execute(new MapEditCommand(Map.Current!, layerIndex, gridCoord, tileId));
					SceneView.RepaintAll();
				}
			}

			if (gridCoord != hoveredGridCoord)
			{
				hoveredGridCoord = gridCoord;
				MapDisplay.SetPreview(new TileRenderInfo(layerIndex, gridCoord, tileId));
				SceneView.RepaintAll();
			}
		}

		HandleSelectorInput(layerIndex, ref tileId);
	}

	private void HandleSelectorInput(int layerIndex, ref int tileId)
	{
		if (directionalInputAction.Begin(out Coord direction) && screenRects != null)
		{
			Vector2 startCenter = screenRects[tileId].Center;
			int closestTileId = tileId;
			float closestDistance = float.MaxValue;

			foreach((int i, Rect rect) in screenRects)
			{
				if (i == tileId)
					continue;

				Vector2 targetCenter = screenRects[i].Center;
				Vector2 diff = targetCenter - startCenter;

				bool isInDirection = (direction.X < 0 && diff.X < 0) ||
				                     (direction.X > 0 && diff.X > 0) ||
				                     (direction.Y < 0 && diff.Y < 0) ||
				                     (direction.Y > 0 && diff.Y > 0);

				if (isInDirection)
				{
					float distance = diff.LengthSquared();
					if (distance < closestDistance)
					{
						closestDistance = distance;
						closestTileId = i;
					}
				}
			}

			tileId = closestTileId;
			if (hoveredGridCoord != null)
			{
				MapDisplay.RefreshPreview(new TileRenderInfo(layerIndex, hoveredGridCoord.Value, tileId));
				SceneView.RepaintAll();
			}
		}
	}

	public override void DrawUI(SpriteAtlas atlas, string textureId)
	{
		var sb = new StringBuilder();

		foreach((int i, Rect rect) in atlas.Sprites)
		{
			sb.Append(i).Append('\0');
		}

		if (ImGui.Combo("Tile", ref tileId, sb.ToString()))
			Prefs.Set("TileBrushWindow.TileId", tileId);

		ImGui.Checkbox("Original Layout", ref keepOriginalLayout);

		ImGui.BeginChild("Tileset");
		DrawTileSelector(atlas, textureId, ref tileId);
		ImGui.EndChild();
	}

	private void DrawTileSelector(SpriteAtlas atlas, string textureId, ref int tileId)
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

		if (screenRects == null || screenRects.Count != atlas.SpriteCount)
			screenRects = new();

		foreach((int i, Rect rect) in atlas.Sprites)
		{
			if (keepOriginalLayout)
				ImGui.SetCursorPos(offset + new Vector2(rect.X, rect.Y) * upscale);

			if (atlas.GetRenderInfo(i, out Vector2 size, out Vector2 uv0, out Vector2 uv1))
				ImGui.Image(tileSetPointer, size * upscale, uv0, uv1);

			// Drawing an image and then checking for the click is much faster than using the ImageButton.
			if (ImGui.IsItemClicked())
			{
				tileId = i;
				Prefs.Set("TileBrushWindow.TileId", tileId);
			}

			screenRects[i] = new Rectangle(
				ImGui.GetItemRectMin().X,
				ImGui.GetItemRectMin().Y,
				ImGui.GetItemRectMax().X - ImGui.GetItemRectMin().X,
				ImGui.GetItemRectMax().Y - ImGui.GetItemRectMin().Y);

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
}