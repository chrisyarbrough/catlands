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

	public static void Draw(AnimationEditorData data)
	{
		if (ImGui.Begin("Timeline"))
		{
			ImGui.DragFloat("Scale", ref xScale, v_speed: 0.01f, v_min: 0.1f, v_max: 100f);

			ImGui.BeginChild("TimelineChild", new Vector2(ImGui.GetContentRegionAvail().X, 170), true, ImGuiWindowFlags.HorizontalScrollbar);
			
			// ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 30);
			Animation animation = data.spriteAtlas.Animations[data.selectedAnimationIndex];

			float pos = ImGui.GetCursorPosX();
			for (float time = 0; time <= animation.Duration; time += 1)
			{
				Vector2 frameStart = ImGui.GetCursorScreenPos();

				Vector2 p = ImGui.GetCursorPos();
				ImGui.SetCursorPos(new Vector2(p.X + 4, p.Y - 4));
				ImGui.Text(time.ToString("N0"));
				ImGui.SetCursorPos(p);

				ImGui.GetWindowDrawList().AddLine(frameStart, frameStart + new Vector2(0, 120), U32Color.FromValue(0.6f));
				ImGui.SetCursorPosX(ImGui.GetCursorPosX() + TimeToPixels(1));
			}
			ImGui.SetCursorPosX(pos);
			
			pos = ImGui.GetCursorPosX();
			ImGui.SetCursorPosX(pos + TimeToPixels(0.5f));
			for (float time = 0f; time < animation.Duration; time += 1f)
			{
				Vector2 frameStart = ImGui.GetCursorScreenPos();
				ImGui.GetWindowDrawList().AddLine(frameStart + new Vector2(0, 10), frameStart + new Vector2(0, 100), U32Color.FromValue(0.4f));
				ImGui.SetCursorPosX(ImGui.GetCursorPosX() + TimeToPixels(1f));
			}
			ImGui.SetCursorPosX(pos);
			
			ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 15);

			DrawTimeline(data.spriteAtlas, animation);
			ImGui.EndChild();
		}

		ImGui.End();
	}

	private static void DrawTimeline(SpriteAtlas spriteAtlas, Animation animation)
	{
		HandleWindowDragging();
		DrawFrames(spriteAtlas, animation);
		HandleFrameResizing(animation);
	}

	private static void HandleWindowDragging()
	{
		if (ImGui.GetIO().MouseDown[(int)ImGuiMouseButton.Right] ||
		    ImGui.GetIO().MouseDown[(int)ImGuiMouseButton.Middle])
		{
			ImGui.SetScrollX(ImGui.GetScrollX() - ImGui.GetIO().MouseDelta.X);
			Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_RESIZE_ALL);
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
			if (ImGui.InputFloat($"##Duration{frameIndex}", ref duration, 0f, 0f, "%.2fs", ImGuiInputTextFlags.EnterReturnsTrue))
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
			if (ImGui.IsMouseHoveringRect(dragAreaStart, dragAreaEnd))
			{
				highlightColor = U32Color.FromValue(1f);

				if (ImGui.GetIO().MouseClicked[(int)ImGuiMouseButton.Left])
				{
					draggedSplitter = frameIndex;
				}

				Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_RESIZE_EW);
			}

			if (frameIndex == draggedSplitter)
				highlightColor = U32Color.Orange;

			if (ImGui.GetIO().MouseReleased[(int)ImGuiMouseButton.Left])
				draggedSplitter = null;

			if (debugDragArea)
				ImGui.GetWindowDrawList().AddRect(dragAreaStart, dragAreaEnd, highlightColor);
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