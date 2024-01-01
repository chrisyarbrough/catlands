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
	private static float viewportHeight = 200;

	private static ClampedFloat xZoom = new(1f, 0.05f, 100f);
	private static int? draggedSplitter;
	private static bool debugDragArea = false;
	private static HashSet<int> selection = new();
	private static Vector2? mouseDownPos;

	private const float dragAreaSize = 8f;

	private static bool isResizingMultipleFrames;
	private readonly SpriteAtlasViewModel data;

	private static readonly IInputAction dragMouseButton = MouseButtonAction.Pan;

	public AnimationTimelineWindow(SpriteAtlasViewModel data) : base("Timeline")
	{
		this.data = data;
	}

	protected override ImGuiWindowFlags SetupWindow()
	{
		return ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
	}

	protected override void DrawContent()
	{
		if (!data.Animations.Any())
			return;

		ImGuiUtil.DragFloat("Zoom", ref xZoom, speed: 0.01f, "%.2f");

		ImGui.BeginChild("TimelineChild", new Vector2(ImGui.GetContentRegionAvail().X, viewportHeight), true,
			ImGuiWindowFlags.HorizontalScrollbar);

		Animation animation = data.SelectedAnimation;

		if (ImGui.IsWindowHovered() && Raylib.GetMouseWheelMove() != 0 && !ImGui.GetIO().KeyShift)
		{
			float zoomFactor = (float)Math.Log(xZoom + 1, 10) * 0.02f;
			xZoom.Value += zoomFactor * Raylib.GetMouseWheelMove();
		}

		var pos = ImGui.GetCursorPos();
		Vector2 cursor = ImGui.GetCursorScreenPos();
		float height = viewportHeight - 60f;
		DrawTimelineGrid(animation, cursor, height);

		ImGui.SetCursorPos(pos);

		ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 31);

		DrawTimeline(data, animation);
		ImGui.EndChild();
	}

	private void DrawTimelineGrid(Animation animation, Vector2 cursor, float height)
	{
		float timeStep = 1f;

		do
		{
			for (float time = 0f; time <= animation.Duration; time += timeStep)
			{
				// Skip the drawing if this time step was already drawn in a previous larger step.
				if (timeStep < 1f && time % (timeStep * 2) == 0)
					continue;

				float xPos = TimeToPixels(time);
				DrawLine(cursor, xPos, height, U32Color.FromValue(MathUtility.Lerp(0.3f, 1f, timeStep)));
				DrawLabel(cursor, xPos, time);
			}

			timeStep /= 2;

			const float lineHeightReduction = 3f;
			height -= lineHeightReduction;
			cursor.Y += lineHeightReduction;
		} while (TimeToPixels(timeStep) > 50 && timeStep > 0.01f);
	}

	private void DrawLine(Vector2 cursor, float xPos, float height, uint color)
	{
		Vector2 startPos = cursor + new Vector2(xPos, 0);
		Vector2 endPos = startPos + new Vector2(0, height);
		ImGui.GetWindowDrawList().AddLine(startPos, endPos, color);
	}

	private void DrawLabel(Vector2 cursor, float xPos, float time)
	{
		ImGui.SetCursorScreenPos(new Vector2(cursor.X + xPos + 4, cursor.Y - 4));
		ImGui.Text(time.ToString("0.##", CultureInfo.InvariantCulture));
		ImGui.SetCursorScreenPos(new Vector2(cursor.X + xPos, cursor.Y - 4));
	}

	private static void DrawTimeline(SpriteAtlasViewModel spriteAtlas, Animation animation)
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

	private static void DrawFrames(SpriteAtlasViewModel spriteAtlas, Animation animation)
	{
		var texturePtr = spriteAtlas.TexturePointer;

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

	private static float TimeToPixels(float duration) => xZoom * pixelsPerSecond * duration;
	private static float PixelsToTime(float pixels) => pixels / pixelsPerSecond / xZoom;

	private static IEnumerable<(int, Animation.Frame, Rectangle)> GetFrameRects(Animation animation)
	{
		float startX = ImGui.GetCursorPosX();
		for (int i = 0; i < animation.FrameCount; i++)
		{
			Animation.Frame frame = animation.FrameAt(i);
			float frameWidth = TimeToPixels(frame.Duration);
			Vector2 frameStart = ImGui.GetCursorScreenPos();
			var rect = new Rectangle(frameStart.X, frameStart.Y, frameWidth, viewportHeight - 90f);
			yield return (i, frame, rect);
			ImGui.SetCursorPosX(ImGui.GetCursorPosX() + frameWidth);
		}

		ImGui.SetCursorPosX(startX);
	}
}