using System.Globalization;
using System.Numerics;
using CatLands;
using CatLands.SpriteEditor;
using ImGuiNET;
using Raylib_cs;

internal class AnimationTimelineWindow : Window
{
	/// <summary>
	/// The baseline scale of the x-axis.
	/// </summary>
	private static float pixelsPerSecond = 400f;

	/// <summary>
	/// The height of the child scroll area in pixels.
	/// </summary>
	private static float viewportHeight = 180;
	
	private static float xScale = 1f;
	private static int? draggedSplitter;
	private static bool debugDragArea = false;
	private static HashSet<int> selection = new();
	private static Vector2? mouseDownPos;

	private static readonly uint[] gridColors = new[] { U32Color.FromValue(0.5f), U32Color.FromValue(0.3f) };
	private const float dragAreaSize = 8f;

	private static bool isResizingMultipleFrames;
	private readonly AnimationEditor data;

	private static readonly IInputAction dragMouseButton = MouseButtonAction.Pan;

	public AnimationTimelineWindow(AnimationEditor data) : base("Timeline")
	{
		this.data = data;
	}

	protected override void DrawContent()
	{
		if (data.SpriteAtlas.Animations.Count == 0)
			return;

		ImGui.DragFloat("Zoom", ref xScale, v_speed: 0.01f, v_min: 0.1f, v_max: 100f);

		ImGui.BeginChild("TimelineChild", new Vector2(ImGui.GetContentRegionAvail().X, viewportHeight), true,
			ImGuiWindowFlags.HorizontalScrollbar);

		Animation animation = data.SpriteAtlas.Animations[data.SelectedAnimationIndex];

		if (ImGui.IsWindowHovered() && Raylib.GetMouseWheelMove() != 0)
		{
			xScale += Raylib.GetMouseWheelMove() * 0.02f;
		}

		float pos = ImGui.GetCursorPosX();
		Vector2 cursor = ImGui.GetCursorScreenPos();
		float height = viewportHeight - 40f;
		for (float time = 0; time <= animation.Duration; time += 1f)
		{
			float xPos = TimeToPixels(time);
			Vector2 startPos = cursor + new Vector2(xPos, 0);
			Vector2 endPos = startPos + new Vector2(0, height);
			ImGui.GetWindowDrawList().AddLine(startPos, endPos, gridColors[0]);

			ImGui.SetCursorScreenPos(new Vector2(cursor.X + xPos + 4, cursor.Y - 4));
			ImGui.Text(time.ToString("N0"));
			ImGui.SetCursorScreenPos(new Vector2(cursor.X + xPos, cursor.Y - 4));

			DrawGridRecursive(cursor, time, time + 1, xPos, TimeToPixels(time + 1f), 0f, height, level: 1, animation.Duration);
		}

		ImGui.SetCursorPosX(pos);

		ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 25);

		DrawTimeline(data.SpriteAtlas, animation);
		ImGui.EndChild();
	}

	private static void DrawGridRecursive(
		Vector2 cursor, float timeLeft, float timeRight, float startX, float endX,
		float yPos, float height, int level, float animationDuration)
	{
		const float minGridStepSize = 50;
		if (endX - startX < minGridStepSize)
			return;

		float heightOffset = height * 0.03f;
		float newYPos = yPos + heightOffset;
		float newHeight = height - heightOffset * 2f;

		float timeMid = (timeLeft + timeRight) / 2f;
		float midX = (startX + endX) / 2f;

		if (timeMid < animationDuration)
		{
			Vector2 startPos = cursor + new Vector2(midX, newYPos);
			Vector2 endPos = startPos with { Y = startPos.Y + newHeight };
			ImGui.GetWindowDrawList().AddLine(startPos, endPos, gridColors[level % 2]);

			if (endX - startX > minGridStepSize * 2f)
			{
				ImGui.SetCursorScreenPos(new Vector2(cursor.X + midX + 4, cursor.Y - 4));
				ImGui.Text(timeMid.ToString("0.##", CultureInfo.InvariantCulture));
				ImGui.SetCursorScreenPos(new Vector2(cursor.X + midX, cursor.Y - 4));
			}
			
			// Draw right side of the segment.
			DrawGridRecursive(cursor, timeMid, timeRight, midX, endX, newYPos, newHeight, level + 1, animationDuration);
		}

		// Recursively draw left side of the segment.
		DrawGridRecursive(cursor, timeLeft, timeMid, startX, midX, newYPos, newHeight, level + 1, animationDuration);
	}

	private static void DrawTimeline(SpriteAtlas spriteAtlas, Animation animation)
	{
		DrawFrames(spriteAtlas, animation);
		HandleFrameResizing(animation);
		HandleBoxSelect(animation);
		HandleDragToScroll();
	}

	private static void HandleBoxSelect(Animation animation)
	{
		if (draggedSplitter != null)
			return;

		Vector2 mousePos = Raylib.GetMousePosition();

		if (selection.Count > 1)
		{
			float duration = animation.Frames.Take(selection.Max() + 1).Sum(x => x.Duration);
			float x = TimeToPixels(duration);
			Vector2 pos = ImGui.GetCursorScreenPos() + new Vector2(x + 20, 0f);
			Vector2 pos2 = pos + new Vector2(0f, 100f);
			ImGui.GetWindowDrawList().AddLine(pos, pos2, U32Color.Red);

			var dragAreaStart = new Vector2(pos.X - dragAreaSize, pos.Y);
			var dragAreaEnd = new Vector2(pos.X + dragAreaSize, pos.Y + 100f);

			if (ImGui.IsMouseHoveringRect(dragAreaStart, dragAreaEnd))
				Cursor.Push(MouseCursor.MOUSE_CURSOR_RESIZE_EW);

			if (debugDragArea)
				ImGui.GetWindowDrawList().AddRect(dragAreaStart, dragAreaEnd, U32Color.Green);

			if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT) &&
			    ImGui.IsMouseHoveringRect(dragAreaStart, dragAreaEnd))
			{
				isResizingMultipleFrames = true;
			}

			if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
			{
				isResizingMultipleFrames = false;
			}

			if (isResizingMultipleFrames)
			{
				float dragDelta = ImGui.GetIO().MouseDelta.X;
				float draggedDurationDelta = PixelsToTime(dragDelta);
				draggedDurationDelta /= selection.Count;
				foreach (int frame in selection)
				{
					animation.FrameAt(frame).Duration += draggedDurationDelta;
				}
			}
		}


		if (ImGui.IsWindowHovered() && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT) &&
		    !isResizingMultipleFrames)
		{
			mouseDownPos = mousePos;
			selection.Clear();
		}


		if (mouseDownPos != null)
		{
			ImGui.GetWindowDrawList().AddRect(mouseDownPos.Value, mousePos, U32Color.Red);
		}

		if (mouseDownPos != null && Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
		{
			foreach ((int frameIndex, Animation.Frame _, Rectangle rect) in GetFrameRects(animation))
			{
				var selectionRect = RectangleExtensions.FromPoints(mouseDownPos.Value, mousePos);
				if (selectionRect.IsPointWithin(rect.Max()))
				{
					selection.Add(frameIndex);
				}
			}

			mouseDownPos = null;
		}
	}

	private static void HandleDragToScroll()
	{
		if (ImGui.IsWindowHovered() && dragMouseButton.IsDown())
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
			uint bgColor = frameIndex % 2 == 0
				? U32Color.FromValue(0.22f, alpha: 0.8f)
				: U32Color.FromValue(0.15f, alpha: 0.8f);
			ImGui.GetWindowDrawList().AddRectFilled(rect.Min(), rect.Max(), bgColor);

			// Draw preview.
			if (spriteAtlas.GetRenderInfo(frame.TileId, out Vector2 size, out Vector2 uv0, out Vector2 uv1))
			{
				size = MathUtility.FitTo(size, rect.Size());
				Vector2 previewPosition = rect.Min() + new Vector2(
					rect.Width / 2f - size.X / 2f, (rect.Height - size.Y) / 2f);
				ImGui.GetWindowDrawList().AddImage(texturePtr, previewPosition, previewPosition + size, uv0, uv1);
			}

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

			if (frameIndex == draggedSplitter || selection.Contains(frameIndex))
				highlightColor = U32Color.Orange;

			if (ImGui.GetIO().MouseReleased[(int)ImGuiMouseButton.Left])
				draggedSplitter = null;

			if (debugDragArea)
				ImGui.GetWindowDrawList().AddRect(dragAreaStart, dragAreaEnd, highlightColor);

			if (isHovered || draggedSplitter == frameIndex || selection.Contains(frameIndex))
			{
				Vector2 start = new Vector2((dragAreaStart.X + dragAreaEnd.X) * 0.5f, dragAreaStart.Y);
				Vector2 end = new Vector2((dragAreaStart.X + dragAreaEnd.X) * 0.5f, dragAreaEnd.Y);
				ImGui.GetWindowDrawList().AddLine(start, end, highlightColor);
			}
		}


		if (selection.Count <= 1 && draggedSplitter != null)
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

	private static float TimeToPixels(float duration) => xScale * pixelsPerSecond * duration;
	private static float PixelsToTime(float pixels) => pixels / pixelsPerSecond / xScale;

	private static IEnumerable<(int, Animation.Frame, Rectangle)> GetFrameRects(Animation animation)
	{
		float startX = ImGui.GetCursorPosX();
		for (int i = 0; i < animation.FrameCount; i++)
		{
			Animation.Frame frame = animation.FrameAt(i);
			float frameWidth = TimeToPixels(frame.Duration);
			Vector2 frameStart = ImGui.GetCursorScreenPos();
			var rect = new Rectangle(frameStart.X, frameStart.Y, frameWidth, viewportHeight - 80f);
			yield return (i, frame, rect);
			ImGui.SetCursorPosX(ImGui.GetCursorPosX() + frameWidth);
		}

		ImGui.SetCursorPosX(startX);
	}
}