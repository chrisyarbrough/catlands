namespace CatLands.SpriteEditor;

using System.Numerics;
using ImGuiNET;
using Raylib_cs;

public class RectangleGizmo
{
	private static bool snapToPixel = true;

	private static int hotControl = -1;
	private static int hoveredControl = -1;
	private static Vector2 accumulatedMouseDelta;

	private static readonly Rectangle[] handleRects = new Rectangle[8];
	private static readonly Vector2[] dimensions = new Vector2[8];

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
		new(0, 0) // TopLeft
	};

	private const float LineWidthWorld = 2f;

	public static Rectangle Draw(Rectangle gizmoRect, Vector2 mousePos, Camera2D camera)
	{
		ImGui.Checkbox("Snap to pixel", ref snapToPixel);

		// To keep the same screen size.
		float scaleFactor = 1.0f / camera.Zoom;
		float lineWidth = LineWidthWorld * scaleFactor;

		// Rectangle nonFlipped = new Rectangle(gizmoRect.xMin(), gizmoRect.yMin(), MathF.Abs(gizmoRect.Width),
		// 	MathF.Abs(gizmoRect.Height));
		Raylib.DrawRectangleLinesEx(gizmoRect, lineWidth, Color.WHITE);

		UpdateHandleRects(gizmoRect, scaleFactor);

		hoveredControl = GetHoveredControl(mousePos, camera);

		if (hoveredControl != -1 && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
		{
			hotControl = hoveredControl;
			accumulatedMouseDelta = Vector2.Zero;

			// When switching from free to snapped mode, align with the pixel grid.
			gizmoRect = ApplySnapping(gizmoRect);
		}

		if (hotControl != -1)
		{
			if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
				gizmoRect = DoMouseDrag(gizmoRect, camera);

			if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
			{
				hotControl = -1;
			}
		}

		for (int i = 0; i < handleRects.Length; i++)
		{
			Raylib.DrawRectangleRec(handleRects[i], GetHandleColor(i));
		}

		return gizmoRect;
	}

	private static Rectangle DoMouseDrag(Rectangle gizmoRect, Camera2D camera)
	{
		Vector2 delta = Raylib.GetMouseDelta();
		delta /= camera.Zoom;
		accumulatedMouseDelta += delta;

		float snappedX = ApplySnapping(accumulatedMouseDelta.X);
		float snappedY = ApplySnapping(accumulatedMouseDelta.Y);

		if (MathF.Abs(snappedX) >= 1 || MathF.Abs(snappedY) >= 1 || !snapToPixel)
		{
			switch (hotControl)
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
			}

			// Carry over the fractional part that exceeds the snap threshold.
			if (snapToPixel)
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
		float shortSize = 15 * scaleFactor;
		float longSize = 25 * scaleFactor;

		// Top, Bottom
		for (int i = 0; i < 4; i += 2)
			dimensions[i] = new Vector2(longSize, shortSize);

		// Right, Left
		for (int i = 1; i < 4; i += 2)
			dimensions[i] = new Vector2(shortSize, longSize);

		// Corners
		for (int i = 4; i < 8; i++)
			dimensions[i] = new Vector2(shortSize, shortSize);

		float halfLineWidth = LineWidthWorld / 2f * scaleFactor;

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

	private static int GetHoveredControl(Vector2 mousePos, Camera2D camera)
	{
		Vector2 mouseWorldPos = Raylib.GetScreenToWorld2D(mousePos, camera);

		for (int i = 0; i < handleRects.Length; i++)
		{
			if (handleRects[i].IsPointWithin(mouseWorldPos))
				return i;
		}

		return -1;
	}

	private static Color GetHandleColor(int controlId)
	{
		return hotControl == controlId ? Color.YELLOW : hoveredControl == controlId ? Color.SKYBLUE : Color.BLUE;
	}

	private static float ApplySnapping(float value)
	{
		if (snapToPixel)
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