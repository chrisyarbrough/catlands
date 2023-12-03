namespace CatLands.SpriteEditor;

using System.Globalization;
using System.Numerics;
using ImGuiNET;
using Raylib_cs;

public enum UpdatePhase
{
	Input,
	Draw
}

public class RectangleGizmo
{
	public static bool SnapToPixel = true;
	public static bool DrawGizmos = true;

	// The handle within this gizmo.
	private static int hotHandle = -1;
	private static int hoveredHandle = -1;
	private static Vector2 accumulatedMouseDelta;

	private static readonly Rectangle[] handleRects = new Rectangle[9];
	private static readonly Vector2[] dimensions = new Vector2[9];

	// To calculate where the handles are positioned relative to the main gizmo center.
	private static readonly Vector2[] offsets =
	{
		new(0.5f, 0), // Top
		new(1, 0.5f), // Right
		new(0.5f, 1), // Bottom
		new(0, 0.5f), // Left
		new(1, 0), // TopRight
		new(1, 1), // BottomRight
		new(0, 1), // BottomLeft
		new(0, 0), // TopLeft
		new(0.5f, 0.5f) // Center
	};

	private static readonly MouseCursor[] cursors =
	{
		MouseCursor.MOUSE_CURSOR_RESIZE_NS,
		MouseCursor.MOUSE_CURSOR_RESIZE_EW,
		MouseCursor.MOUSE_CURSOR_RESIZE_NS,
		MouseCursor.MOUSE_CURSOR_RESIZE_EW,
		MouseCursor.MOUSE_CURSOR_RESIZE_NESW,
		MouseCursor.MOUSE_CURSOR_RESIZE_NWSE,
		MouseCursor.MOUSE_CURSOR_RESIZE_NESW,
		MouseCursor.MOUSE_CURSOR_RESIZE_NWSE,
		MouseCursor.MOUSE_CURSOR_RESIZE_ALL
	};

	private const float lineWidthWorld = 2f;

	static RectangleGizmo()
	{
		SettingsWindow.Add("Gizmos", () =>
		{
			ImGui.Checkbox("Snap to pixel (S)", ref SnapToPixel);
			if (Raylib.IsKeyPressed(KeyboardKey.KEY_S))
				SnapToPixel = !SnapToPixel;

			ImGui.Checkbox("Draw Gizmos (G)", ref DrawGizmos);
			if (Raylib.IsKeyPressed(KeyboardKey.KEY_G))
				DrawGizmos = !DrawGizmos;
		});
	}

	public static Rectangle Draw(
		Rectangle gizmoRect, int controlId, Vector2 mouseWorldPos, Camera2D camera, SpriteAtlas spriteAtlas,
		UpdatePhase phase, bool isHovered)
	{
		// To keep the same screen size.
		float scaleFactor = 1.0f / camera.Zoom;

		// If the rectangle is too tiny on screen, there's no use trying to draw or select it.
		float screenSize = MathF.Max(gizmoRect.Width * camera.Zoom, gizmoRect.Height * camera.Zoom);
		if (screenSize < 8)
			return gizmoRect;

		if (phase == UpdatePhase.Draw)
		{
			Color color = controlId == GuiUtility.HotControl ? Color.ORANGE :
				Selection.IsSelected(controlId) ? Color.ORANGE : Color.RAYWHITE;

			if (Selection.IsSelected(controlId))
				Raylib.DrawRectangleLinesEx(gizmoRect.GrowBy(lineWidthWorld * 1f * scaleFactor),
					lineWidthWorld * 2f * scaleFactor, Color.BLACK);

			Raylib.DrawRectangleLinesEx(gizmoRect.GrowBy(lineWidthWorld * 0.5f * scaleFactor),
				lineWidthWorld * scaleFactor, color);
		}

		if (screenSize < 14)
			return gizmoRect;

		// Disable input if hovering over a window.
		if (ImGui.GetIO().WantCaptureMouse)
			return gizmoRect;

		if (phase == UpdatePhase.Input)
		{
			UpdateHandleRects(gizmoRect, scaleFactor);

			hoveredHandle = GetHoveredControl(mouseWorldPos);

			if (hoveredHandle != -1 && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT) &&
			    GuiUtility.HotControl == -1)
			{
				UndoManager.RecordSnapshot(spriteAtlas);

				hotHandle = hoveredHandle;
				accumulatedMouseDelta = Vector2.Zero;
				GuiUtility.HotControl = controlId;
				Selection.SetSingleSelection(controlId);

				// When switching from free to snapped mode, align with the pixel grid.
				gizmoRect = ApplySnapping(gizmoRect);
			}

			if (hotHandle != -1)
			{
				if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT) && GuiUtility.HotControl == controlId)
				{
					gizmoRect = DoMouseDrag(gizmoRect, camera);
					DrawSizeLabels(gizmoRect, scaleFactor);
				}

				Raylib.SetMouseCursor(cursors[hotHandle]);

				if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
				{
					hotHandle = -1;
					GuiUtility.HotControl = -1;
				}
			}
			else if (hoveredHandle != -1)
			{
				Raylib.DrawRectangleRec(handleRects[hoveredHandle], GetHandleColor(hoveredHandle));
				Raylib.SetMouseCursor(cursors[hoveredHandle]);
			}
		}

		if (isHovered && phase == UpdatePhase.Draw)
		{
			for (int i = 4; i < handleRects.Length; i++)
			{
				Raylib.DrawRing(handleRects[i].Center(), 3f * scaleFactor, 6f * scaleFactor, 0f, 360f, 16, Color.BLUE);
			}
		}

		return gizmoRect;
	}

	private static void DrawSizeLabels(Rectangle gizmoRect, float scaleFactor)
	{
		int fontSize = (int)(28 * scaleFactor);
		string formatString = SnapToPixel ? "0" : "0.00";
		Vector2 shadowOffset = new Vector2(1, 1) * scaleFactor;

		Vector2 pos = handleRects[0].Center();
		string text = gizmoRect.Width.ToString(formatString, CultureInfo.InvariantCulture);
		Vector2 textSize = LabelUtility.Measure(text, fontSize);
		pos.X -= textSize.X / 2;
		pos.Y -= textSize.Y + 4 * scaleFactor;
		LabelUtility.Draw(text, pos, shadowOffset, fontSize);

		pos = handleRects[1].Center();
		pos.X += 8 * scaleFactor;
		pos.Y -= textSize.Y / 2;
		text = gizmoRect.Height.ToString(formatString, CultureInfo.InvariantCulture);
		LabelUtility.Draw(text, pos, shadowOffset, fontSize);
	}

	private static Rectangle DoMouseDrag(Rectangle gizmoRect, Camera2D camera)
	{
		Vector2 delta = Raylib.GetMouseDelta();
		delta /= camera.Zoom;
		accumulatedMouseDelta += delta;

		float snappedX = ApplySnapping(accumulatedMouseDelta.X);
		float snappedY = ApplySnapping(accumulatedMouseDelta.Y);

		if (MathF.Abs(snappedX) >= 1 || MathF.Abs(snappedY) >= 1 || !SnapToPixel)
		{
			switch (hotHandle)
			{
				case 0: // Top
					gizmoRect.Y += snappedY;
					gizmoRect.Height -= snappedY;
					break;
				case 1: // Right
					gizmoRect.Width += snappedX;
					break;
				case 2: // Bottom
					gizmoRect.Height += snappedY;
					break;
				case 3: // Left
					gizmoRect.X += snappedX;
					gizmoRect.Width -= snappedX;
					break;
				case 4: // TopRight
					gizmoRect.Y += snappedY;
					gizmoRect.Height -= snappedY;
					gizmoRect.Width += snappedX;
					break;
				case 5: // BottomRight
					gizmoRect.Height += snappedY;
					gizmoRect.Width += snappedX;
					break;
				case 6: // BottomLeft
					gizmoRect.Height += snappedY;
					gizmoRect.X += snappedX;
					gizmoRect.Width -= snappedX;
					break;
				case 7: // TopLeft
					gizmoRect.Y += snappedY;
					gizmoRect.Height -= snappedY;
					gizmoRect.X += snappedX;
					gizmoRect.Width -= snappedX;
					break;
				case 8: // Center
					gizmoRect.X += snappedX;
					gizmoRect.Y += snappedY;
					break;
			}

			// Carry over the fractional part that exceeds the snap threshold.
			if (SnapToPixel)
			{
				accumulatedMouseDelta.X -= MathF.Sign(snappedX) * MathF.Floor(MathF.Abs(snappedX));
				accumulatedMouseDelta.Y -= MathF.Sign(snappedY) * MathF.Floor(MathF.Abs(snappedY));
			}
			else
			{
				accumulatedMouseDelta = Vector2.Zero;
			}
		}

		return gizmoRect;
	}

	private static void UpdateHandleRects(Rectangle gizmoRect, float scaleFactor)
	{
		float grabAreaSize = 15 * scaleFactor;

		// Top, Bottom
		for (int i = 0; i < 4; i += 2)
			dimensions[i] = new Vector2(gizmoRect.Width - grabAreaSize, grabAreaSize);

		// Right, Left
		for (int i = 1; i < 4; i += 2)
			dimensions[i] = new Vector2(grabAreaSize, gizmoRect.Height - grabAreaSize);

		// Corners
		for (int i = 4; i < 8; i++)
			dimensions[i] = new Vector2(grabAreaSize, grabAreaSize);

		// Center
		dimensions[8] = new Vector2(grabAreaSize * 2f, grabAreaSize * 2f);

		// When drawing circles, we want them centered on the actual vector position corner,
		// but when drawing a rectangle, we would want it to be offset so that it appears centered visually.
		const float halfLineWidth = 0f; //lineWidthWorld / 2f * scaleFactor;

		for (int i = 0; i < handleRects.Length; i++)
		{
			Vector2 direction = offsets[i] - new Vector2(0.5f, 0.5f);

			float posX = gizmoRect.X + gizmoRect.Width * offsets[i].X - dimensions[i].X / 2;
			float posY = gizmoRect.Y + gizmoRect.Height * offsets[i].Y - dimensions[i].Y / 2;
			handleRects[i] = new Rectangle(
				posX - MathF.Sign(direction.X) * halfLineWidth,
				posY - MathF.Sign(direction.Y) * halfLineWidth,
				dimensions[i].X,
				dimensions[i].Y);
		}
	}

	private static int GetHoveredControl(Vector2 mouseWorldPos)
	{
		for (int i = 0; i < handleRects.Length; i++)
		{
			if (handleRects[i].IsPointWithin(mouseWorldPos))
				return i;
		}

		return -1;
	}

	private static Color GetHandleColor(int controlId)
	{
		Color color = hotHandle == controlId ? Color.YELLOW : hoveredHandle == controlId ? Color.SKYBLUE : Color.BLUE;
		color.A = 128;
		return color;
	}

	private static float ApplySnapping(float value)
	{
		if (SnapToPixel)
			return (float)Math.Round(value);

		return value;
	}

	private static Rectangle ApplySnapping(Rectangle rect)
	{
		rect.X = ApplySnapping(rect.X);
		rect.Y = ApplySnapping(rect.Y);
		rect.Width = ApplySnapping(rect.Width);
		rect.Height = ApplySnapping(rect.Height);
		return rect;
	}
}