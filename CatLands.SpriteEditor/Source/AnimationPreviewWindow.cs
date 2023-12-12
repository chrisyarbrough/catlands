using System.Numerics;
using CatLands.SpriteEditor;
using ImGuiNET;
using Raylib_cs;

internal class AnimationPreviewWindow
{
	private static IntPtr texturePtr;
	private static AnimationPlayer player = new();
	private static int lastSelectedIndex = -1;

	public static void Draw(SpriteAtlas spriteAtlas, int selectedAnimationIndex)
	{
		if (ImGui.Begin("Animation Preview"))
		{
			Animation animation = spriteAtlas.Animations[selectedAnimationIndex];
			
			if (selectedAnimationIndex != lastSelectedIndex)
			{
				player.SetAnimation(animation);
				texturePtr = new IntPtr(spriteAtlas.Texture.Id);
				lastSelectedIndex = selectedAnimationIndex;
			}
			else
			{
				ImGui.DragFloat("Speed", ref player.Speed, v_speed: 0.1f, float.MinValue, float.MaxValue);

				if (player.Update(Raylib.GetFrameTime(), out int frame))
				{
					int tileId = animation.FrameAt(frame).TileId;
					spriteAtlas.GetRenderInfo(tileId, out Vector2 size, out Vector2 uv0, out Vector2 uv1);
					size = FitToWindow(size);
					ImGui.Image(texturePtr, size, uv0, uv1);
				}
			}
		}

		ImGui.End();
	}

	private static Vector2 FitToWindow(Vector2 size)
	{
		Vector2 contentRegion = ImGui.GetContentRegionAvail();

		float scaleX = contentRegion.X / size.X;
		float scaleY = contentRegion.Y / size.Y;
		float minScale = Math.Min(scaleX, scaleY);

		return new Vector2(size.X * minScale, size.Y * minScale);
	}
}