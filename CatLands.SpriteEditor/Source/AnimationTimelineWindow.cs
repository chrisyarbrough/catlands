using System.Globalization;
using System.Numerics;
using CatLands;
using CatLands.SpriteEditor;
using ImGuiNET;
using Raylib_cs;

internal class AnimationTimelineWindow
{
	private static float timelineHeight = 80f;
	private static float xScale = 1f;
	private static int? draggedSplitter;
	private static bool debugDragArea = false;

	private static readonly uint[] gridColors = new[] { U32Color.FromValue(0.5f), U32Color.FromValue(0.3f) };

	public static void Draw(AnimationEditorData data)
	{
		if (ImGui.Begin("Timeline"))
		{
			ImGui.DragFloat("Zoom", ref xScale, v_speed: 0.01f, v_min: 0.1f, v_max: 100f);

			ImGui.BeginChild("TimelineChild", new Vector2(ImGui.GetContentRegionAvail().X, 170), true,
				ImGuiWindowFlags.HorizontalScrollbar);

			Animation animation = data.spriteAtlas.Animations[data.selectedAnimationIndex];
			
			if (Raylib.GetMouseWheelMove() != 0)
			{
				xScale += Raylib.GetMouseWheelMove() * 0.02f;
			}

			float pos = ImGui.GetCursorPosX();
			Vector2 cursor = ImGui.GetCursorScreenPos();
			float height = 120;
			for (float time = 0; time <= animation.Duration; time += 1f)
			{
				float xPos = TimeToPixels(time);
				Vector2 startPos = cursor + new Vector2(xPos, 0);
				Vector2 endPos = startPos + new Vector2(0, height);
				ImGui.GetWindowDrawList().AddLine(startPos, endPos, gridColors[0]);

				ImGui.SetCursorScreenPos(new Vector2(cursor.X + xPos + 4, cursor.Y - 4));
				ImGui.Text(time.ToString("N0"));
				ImGui.SetCursorScreenPos(new Vector2(cursor.X + xPos, cursor.Y - 4));

				DrawGridRecursive(cursor, time, time + 1, xPos, TimeToPixels(time + 1f), 0f, height, level: 1);
			}

			ImGui.SetCursorPosX(pos);

			ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 25);

			DrawTimeline(data.spriteAtlas, animation);
			ImGui.EndChild();
		}

		ImGui.End();
	}

	private static void DrawGridRecursive(Vector2 cursor, float timeLeft, float timeRight, float startX, float endX,
		float yPos, float height, int level)
	{
		const float minGridStepSize = 50;
		if (endX - startX < minGridStepSize)
			return;

		float heightOffset = height * 0.03f;
		float newYPos = yPos + heightOffset;
		float newHeight = height - heightOffset * 2f;

		// Draw mid.
		float xMid = (startX + endX) * 0.5f;
		Vector2 startPos = cursor + new Vector2(xMid, newYPos);
		Vector2 endPos = new Vector2(startPos.X, startPos.Y + newHeight);
		ImGui.GetWindowDrawList().AddLine(startPos, endPos, gridColors[level % 2]);

		float timeMid = (timeLeft + timeRight) / 2f;

		if (endX - startX > minGridStepSize * 2f)
		{
			ImGui.SetCursorScreenPos(new Vector2(cursor.X + xMid + 4, cursor.Y - 4));
			ImGui.Text(timeMid.ToString("0.##", CultureInfo.InvariantCulture));
			ImGui.SetCursorScreenPos(new Vector2(cursor.X + xMid, cursor.Y - 4));
		}

		float mid = (startX + endX) / 2f;
		DrawGridRecursive(cursor, timeLeft, timeMid, startX, mid, newYPos, newHeight, level + 1);
		DrawGridRecursive(cursor, timeMid, timeRight, mid, endX, newYPos, newHeight, level + 1);
	}

	private static void DrawTimeline(SpriteAtlas spriteAtlas, Animation animation)
	{
		DrawFrames(spriteAtlas, animation);
		HandleFrameResizing(animation);
		HandleWindowDragging();
	}

	private static void HandleWindowDragging()
	{
		if (ImGui.GetIO().MouseDown[(int)ImGuiMouseButton.Right] ||
		    ImGui.GetIO().MouseDown[(int)ImGuiMouseButton.Middle])
		{
			ImGui.SetScrollX(ImGui.GetScrollX() - ImGui.GetIO().MouseDelta.X);
			Cursor.Push(MouseCursor.MOUSE_CURSOR_RESIZE_ALL);
		}
	}

	private static void DrawFrames(SpriteAtlas spriteAtlas, Animation animation)
	{
		var texturePtr = new IntPtr(spriteAtlas.Texture.Id);

		foreach ((int frameIndex, Animation.Frame frame, Rectangle rect) in GetFrameRects(animation))
		{
			uint bgColor = frameIndex % 2 == 0 ? U32Color.FromValue(0.22f) : U32Color.FromValue(0.15f);
			ImGui.GetWindowDrawList().AddRectFilled(rect.Min(), rect.Max(), bgColor);

			// Draw preview.
			spriteAtlas.GetRenderInfo(frame.TileId, out Vector2 size, out Vector2 uv0, out Vector2 uv1);
			size = MathUtility.FitTo(size, rect.Size());
			Vector2 previewPosition = rect.Min() + new Vector2(
				rect.Width / 2f - size.X / 2f, (timelineHeight - size.Y) / 2f);
			ImGui.GetWindowDrawList().AddImage(texturePtr, previewPosition, previewPosition + size, uv0, uv1);


			// Draw duration field below preview.
			Vector2 pos = ImGui.GetCursorPos();

			float fieldWidth = Math.Min(45, rect.Width);
			ImGui.SetNextItemWidth(fieldWidth);
			ImGui.SetCursorScreenPos(new Vector2(rect.Center().X - fieldWidth / 2f, rect.Max().Y + 4));

			float duration = frame.Duration;
			if (ImGui.InputFloat($"##Duration{frameIndex}", ref duration, 0f, 0f, "%.2fs",
				    ImGuiInputTextFlags.EnterReturnsTrue))
				frame.Duration = duration;

			ImGui.SetCursorPos(pos);
		}
	}

	private static void HandleFrameResizing(Animation animation)
	{
		// Resize a frame rect to modify the duration.
		foreach ((int frameIndex, Animation.Frame _, Rectangle rect) in GetFrameRects(animation))
		{
			const float dragAreaSize = 8f;
			var dragAreaStart = new Vector2(rect.xMax() - dragAreaSize, rect.yMin());
			var dragAreaEnd = new Vector2(rect.xMax() + dragAreaSize, rect.yMax());

			uint highlightColor = U32Color.FromValue(0.8f);
			bool isHovered = false;

			if (ImGui.IsMouseHoveringRect(dragAreaStart, dragAreaEnd))
			{
				highlightColor = U32Color.FromValue(1f);

				if (ImGui.GetIO().MouseClicked[(int)ImGuiMouseButton.Left])
				{
					draggedSplitter = frameIndex;
				}

				Cursor.Push(MouseCursor.MOUSE_CURSOR_RESIZE_EW);
				isHovered = true;
			}

			if (frameIndex == draggedSplitter)
				highlightColor = U32Color.Orange;

			if (ImGui.GetIO().MouseReleased[(int)ImGuiMouseButton.Left])
				draggedSplitter = null;

			if (debugDragArea)
				ImGui.GetWindowDrawList().AddRect(dragAreaStart, dragAreaEnd, highlightColor);

			if (isHovered || draggedSplitter == frameIndex)
			{
				Vector2 start = new Vector2((dragAreaStart.X + dragAreaEnd.X) * 0.5f, dragAreaStart.Y);
				Vector2 end = new Vector2((dragAreaStart.X + dragAreaEnd.X) * 0.5f, dragAreaEnd.Y);
				ImGui.GetWindowDrawList().AddLine(start, end, highlightColor);
			}
		}

		if (draggedSplitter != null)
		{
			// TODO: Deal with drag offset when scroll offset is changing.
			float dragDelta = ImGui.GetIO().MouseDelta.X;
			float draggedDurationDelta = PixelsToTime(dragDelta);
			animation.FrameAt(draggedSplitter.Value).Duration += draggedDurationDelta;

			if (ImGui.GetIO().KeyAlt && (draggedSplitter.Value + 1) < animation.FrameCount)
			{
				animation.FrameAt(draggedSplitter.Value + 1).Duration -= draggedDurationDelta;
			}
		}
	}

	private static float TimeToPixels(float duration) => xScale * timelineHeight * duration;
	private static float PixelsToTime(float pixels) => pixels / timelineHeight / xScale;

	private static IEnumerable<(int, Animation.Frame, Rectangle)> GetFrameRects(Animation animation)
	{
		float startX = ImGui.GetCursorPosX();
		for (int i = 0; i < animation.FrameCount; i++)
		{
			Animation.Frame frame = animation.FrameAt(i);
			float frameWidth = TimeToPixels(frame.Duration);
			Vector2 frameStart = ImGui.GetCursorScreenPos();
			var rect = new Rectangle(frameStart.X, frameStart.Y, frameWidth, timelineHeight);
			yield return (i, frame, rect);
			ImGui.SetCursorPosX(ImGui.GetCursorPosX() + frameWidth);
		}

		ImGui.SetCursorPosX(startX);
	}
}