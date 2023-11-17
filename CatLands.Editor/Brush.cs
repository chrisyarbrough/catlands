namespace CatLands.Editor;

using System.Numerics;
using System.Text;
using ImGuiNET;
using Raylib_cs;

public class Brush : GameObject
{
	private readonly Map map;
	private readonly MapDisplay mapDisplay;
	public int TileId;
	private Coord? selectedCoord;

	public Brush(Map map, MapDisplay mapDisplay)
	{
		this.map = map;
		this.mapDisplay = mapDisplay;
	}

	public override void Update()
	{
		if (SceneView.Current == null)
			return;

		Vector2 mouseWorldPosition = SceneView.Current.GetMouseWorldPosition();
		Coord gridPosition = Grid.WorldToCoord(mouseWorldPosition);
		Coord snappedScreenPosition = Grid.CoordToWorld(gridPosition);
		DrawOutline(snappedScreenPosition, Grid.TileRenderSize, 2);

		if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
		{
			if (selectedCoord != gridPosition)
			{
				selectedCoord = gridPosition;
				map.Set(gridPosition, tileId: TileId, tilesetId: 0);
			}
		}
	}

	private static void DrawOutline(Coord screenPosition, int rectSize, float lineThickness)
	{
		var outline = new Rectangle(
			screenPosition.X - lineThickness,
			screenPosition.Y - lineThickness,
			rectSize + lineThickness * 2,
			rectSize + lineThickness * 2);
		Raylib.DrawRectangleLinesEx(outline, lineThickness, Color.RED);
	}
	int tileX = 0;
	int tileY = 0;
	public override void OnGui(bool isActive)
	{
		ImGui.Begin("Tools");

		if (!ImGui.IsWindowCollapsed())
		{
			var sb = new StringBuilder();
			int xTileCount = mapDisplay.textures[0].width / Grid.TileSourceSize;
			int yTileCount = mapDisplay.textures[0].height / Grid.TileSourceSize;

			for (int y = 0; y < yTileCount; y++)
			{
				for (int x = 0; x < xTileCount; x++)
				{
					int tileIndex = x + y * xTileCount;
					sb.Append(tileIndex).Append('\0');
				}
			}

			ImGui.Combo("Tile Id", ref TileId, sb.ToString());

			Texture2D tileset = mapDisplay.textures[0];
			Vector2 tilesetSize = new Vector2(tileset.width, tileset.height) * 2f;
			Vector2 tileSize = new Vector2(16, 16) * 2;



			Vector2 pos = ImGui.GetCursorPos();
			ImGui.Image(new IntPtr(tileset.id), tilesetSize);
			{




				tileX = TileId % 10;
				tileY = TileId / 10;


				// Draw a rectangle around the selected tile
				ImGui.GetWindowDrawList().AddRect(
					ImGui.GetItemRectMin() + new Vector2(tileX * tileSize.X, tileY * tileSize.Y),
					ImGui.GetItemRectMin() + new Vector2((tileX + 1) * tileSize.X, (tileY + 1) * tileSize.Y),
					ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 0.0f, 0.0f, 1.0f)) // Red color for the rectangle
				);
			}
			ImGui.SetCursorPos(pos);
			ImGui.InvisibleButton(string.Empty, tilesetSize);
			if (ImGui.IsItemHovered())
			{
				if (ImGui.GetMouseClickedCount(ImGuiMouseButton.Left) == 1)
				{
					// Get the mouse position relative to the image
					Vector2 clickPos = ImGui.GetMousePos() - ImGui.GetItemRectMin();

					// Calculate which tile was clicked
					tileX = (int)(clickPos.X / tileSize.X);
					tileY = (int)(clickPos.Y / tileSize.Y);

					// Check if the click is within the bounds of your tileset
					if (tileX >= 0 && tileX < tilesetSize.X / tileSize.X &&
					    tileY >= 0 && tileY < tilesetSize.Y / tileSize.Y)
					{
						// Tile index calculation
						TileId = tileY * ((int)tilesetSize.X / (int)tileSize.X) + tileX;
					}
				}
			}
		}


		ImGui.End();
	}
}