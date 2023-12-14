using System.Numerics;
using CatLands;
using CatLands.SpriteEditor;
using ImGuiNET;

internal class AnimationTimelineWindow
{
	private static float timelineHeight = 80f;
	private static float xScale = 1f;

	public static void Draw(AnimationEditorData data)
	{
		if (ImGui.Begin("Timeline"))
		{
			Animation animation = data.spriteAtlas.Animations[data.selectedAnimationIndex];
			
			for (int i = 0; i < animation.FrameCount; i++)
			{
				Animation.Frame frame = animation.FrameAt(i);
				data.spriteAtlas.GetRenderInfo(frame.TileId, out Vector2 size, out Vector2 uv0, out Vector2 uv1);

				float frameWidth = xScale * timelineHeight * frame.Duration;

				uint color = i % 2 == 0 ? ImGui.GetColorU32(ImGuiCol.FrameBg) : ImGui.GetColorU32(ImGuiCol.PopupBg);

				Vector2 frameSize = new Vector2(frameWidth, timelineHeight);
				ImGui.GetWindowDrawList().AddRectFilled(ImGui.GetCursorScreenPos(), ImGui.GetCursorScreenPos() + frameSize, color, 0, 0);

				size = FitTo(size, frameSize);
				Vector2 imagePosition = ImGui.GetCursorScreenPos() + new Vector2(frameSize.X * 0.5f - size.X * 0.5f, (timelineHeight - size.Y) / 2);
				ImGui.GetWindowDrawList().AddImage(new IntPtr(data.spriteAtlas.Texture.Id), imagePosition, imagePosition + size, uv0, uv1);

				ImGui.SetCursorPosX(ImGui.GetCursorPosX() + frameWidth);
			}
		}

		ImGui.End();
	}
	
	private static Vector2 FitTo(Vector2 size, Vector2 frameSize)
	{
		float scaleX = frameSize.X / size.X;
		float scaleY = frameSize.Y / size.Y;
		float minScale = Math.Min(scaleX, scaleY);

		return new Vector2(size.X * minScale, size.Y * minScale);
	}
}