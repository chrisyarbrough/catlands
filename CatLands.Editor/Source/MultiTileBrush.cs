namespace CatLands.Editor;

using System.Numerics;
using ImGuiNET;
using Raylib_cs;

public class MultiTileBrush : Brush
{
	public override string DisplayName => "Multi";

	private readonly HashSet<int> selectedTiles = new();

	// In texture space.
	private Vector2 selectionStart;
	private Vector2 selectionEnd;
	private bool isSelecting;

	// Grid coordinates.
	private Coord? lastHoveredGridCoord;
	private Coord? drawnGridCoord;

	public override void Update(SpriteAtlas atlas, int layerIndex, bool mouseOverWindow)
	{
		if (mouseOverWindow)
		{
			Vector2 mouseWorldPosition = SceneView.Current!.GetMouseWorldPosition();
			Coord gridCoord = Grid.WorldToCoord(mouseWorldPosition);

			if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
			{
				if (gridCoord != drawnGridCoord)
				{
					drawnGridCoord = gridCoord;
					CommandManager.Execute(new MapEditCommand(Map.Current!, layerIndex,
						BrushTiles(atlas.SpriteRects, gridCoord)));

					SceneView.RepaintAll();
				}
			}

			if (gridCoord != lastHoveredGridCoord)
			{
				lastHoveredGridCoord = gridCoord;
				MapDisplay.ClearPreviews();

				foreach ((Coord coord, int tileId) in BrushTiles(atlas.SpriteRects, gridCoord))
					MapDisplay.AddPreview(new TileRenderInfo(layerIndex, coord, tileId));

				SceneView.RepaintAll();
			}
		}
	}

	private IEnumerable<(Coord, int)> BrushTiles(IList<Rectangle> spriteRects, Coord gridCoord)
	{
		Coord c = gridCoord;
		Vector2? start = null;

		foreach (int tileId in selectedTiles.OrderBy(x => x))
		{
			if (start == null)
			{
				start = spriteRects[tileId].Center();
			}
			else
			{
				// TODO: Handle freeform vs grid-based tilesets.
				Vector2 current = spriteRects[tileId].Center();
				Vector2 offset = (current - start.Value) / 16f;
				Coord gridOffset = new((int)offset.X, (int)offset.Y);
				c = gridCoord + gridOffset;
			}

			yield return (c, tileId);
		}
	}

	public override void DrawUI(SpriteAtlas atlas, string textureId)
	{
		Texture2D tileset = MapTextures.GetTexture(textureId);
		var tileSetPointer = new IntPtr(tileset.Id);

		Vector2 offset = ImGui.GetWindowPos() + ImGui.GetCursorPos();
		const int upscale = 2;

		ImGui.Image(tileSetPointer, tileset.Size() * upscale);

		Vector2 textureCoord = (ImGui.GetMousePos() - offset) / upscale;

		if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && ImGui.IsWindowHovered())
		{
			selectionStart = textureCoord;
			isSelecting = true;
			selectedTiles.Clear();
		}

		if (isSelecting)
		{
			selectionEnd = textureCoord;

			ImGui.GetWindowDrawList().AddRect(
				selectionStart * upscale + offset,
				selectionEnd * upscale + offset,
				ImGui.GetColorU32(ImGuiCol.Border));
		}

		if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && isSelecting)
		{
			ProcessSelection(atlas);
			isSelecting = false;
		}

		foreach (int selectedTile in selectedTiles)
		{
			Rectangle rect = atlas.SpriteRects[selectedTile];
			ImGui.GetWindowDrawList().AddRect(
				rect.Min() * upscale + offset,
				rect.Max() * upscale + offset,
				ImGui.GetColorU32(ImGuiCol.NavHighlight));
		}
	}

	private void ProcessSelection(SpriteAtlas atlas)
	{
		Rectangle selectionRect = new(
			Math.Min(selectionStart.X, selectionEnd.X),
			Math.Min(selectionStart.Y, selectionEnd.Y),
			Math.Abs(selectionEnd.X - selectionStart.X),
			Math.Abs(selectionEnd.Y - selectionStart.Y));

		for (int i = 0; i < atlas.SpriteRects.Count; i++)
		{
			Rectangle spriteRect = atlas.SpriteRects[i];

			if (selectionRect.Encloses(spriteRect))
			{
				selectedTiles.Add(i);
			}
		}
	}
}