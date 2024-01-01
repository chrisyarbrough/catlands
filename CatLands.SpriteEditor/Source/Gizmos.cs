namespace CatLands.SpriteEditor;

using System.Numerics;
using ImGuiNET;
using Raylib_cs;

internal static class Gizmos
{
	public static int? HoveredControl => hoveredControl != -1 ? hoveredControl : null;
	
	static List<int> hoveredRects = new();
	static int hoveredControl = -1;

	public static void Draw(CameraController camera, SpriteAtlasViewModel viewModel)
	{
		Rectangle worldBounds = camera.GetWorldBounds();
		Vector2 mouseWorldPos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera.State);

		hoveredControl = -1;
		hoveredRects.Clear();

		if (GuiUtility.HotControl == -1)
		{
			// Draw hovered controls first to capture input.
			foreach ((int i, Rect rect) in viewModel.Sprites)
			{
				if (Raylib.CheckCollisionRecs(viewModel[i], worldBounds))
				{
					if (viewModel[i].Contains(mouseWorldPos))
					{
						hoveredRects.Add(i);
					}
				}
			}

			// Choose the rect with the smallest area.
			if (hoveredRects.Count > 0 && !ImGui.GetIO().WantCaptureMouse)
			{
				hoveredControl = hoveredRects.MinBy(i => viewModel[i].Area);
				viewModel[hoveredControl] = RectangleGizmo.Draw(
					viewModel[hoveredControl], hoveredControl, mouseWorldPos, camera.State,
					UpdatePhase.Input, isHovered: true);
			}
		}

		if (hoveredControl == -1 && viewModel.HasSprite(GuiUtility.HotControl))
		{
			viewModel[GuiUtility.HotControl] = RectangleGizmo.Draw(
				viewModel[GuiUtility.HotControl], GuiUtility.HotControl, mouseWorldPos,
				camera.State, UpdatePhase.Input, isHovered: true);
		}

		// Draw default rects
		foreach ((int i, Rect rect) in viewModel.Sprites)
		{
			if (Raylib.CheckCollisionRecs(viewModel[i], worldBounds))
			{
				if (i != hoveredControl && !TileSelection.IsSelected(i))
				{
					viewModel[i] = RectangleGizmo.Draw(
						viewModel[i], i, mouseWorldPos, camera.State, UpdatePhase.Draw,
						isHovered: false);
				}
			}
		}

		// Draw selection
		foreach ((int i, Rect rect) in viewModel.Sprites)
		{
			if (Raylib.CheckCollisionRecs(viewModel[i], worldBounds))
			{
				if (TileSelection.IsSelected(i))
				{
					viewModel[i] = RectangleGizmo.Draw(
						viewModel[i], i, mouseWorldPos, camera.State, UpdatePhase.Draw,
						isHovered: false);
				}
			}
		}

		// Draw hovered control
		if (hoveredControl != -1)
		{
			viewModel[hoveredControl] = RectangleGizmo.Draw(
				viewModel[hoveredControl], hoveredControl, mouseWorldPos, camera.State,
				UpdatePhase.Draw, isHovered: true);
		}

		// Draw hot control
		if (viewModel.HasSprite(GuiUtility.HotControl))
		{
			viewModel[GuiUtility.HotControl] = RectangleGizmo.Draw(
				viewModel[GuiUtility.HotControl], GuiUtility.HotControl, mouseWorldPos,
				camera.State, UpdatePhase.Draw, isHovered: true);
		}

		BoxSelect.Draw(camera.State, controlId: int.MaxValue, rectangle =>
		{
			TileSelection.ClearSelection();
			foreach ((int i, Rect rect) in viewModel.Sprites)
			{
				if (rectangle.Encloses(viewModel[i]))
				{
					TileSelection.AddToSelection(i);
				}
			}
		});
	}
}