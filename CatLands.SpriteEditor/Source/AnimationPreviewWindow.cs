using System.Numerics;
using CatLands;
using CatLands.Editor;
using ImGuiNET;
using Raylib_cs;

internal class AnimationPreviewWindow
{
	private static IntPtr texturePtr;
	private static IAnimationPlayer? player;
	private static int lastSelectedIndex = -1;
	private static Texture2D playIcon, pauseIcon;
	private static readonly Pref<bool> autoPlayPref = new("AutoPlay");

	static AnimationPreviewWindow()
	{
		playIcon = Raylib.LoadTexture("play.png");
		pauseIcon = Raylib.LoadTexture("pause.png");
	}

	public static void Draw(SpriteAtlas spriteAtlas, int selectedAnimationIndex)
	{
		if (ImGui.Begin("Animation Preview"))
		{
			Animation animation = spriteAtlas.Animations[selectedAnimationIndex];

			if (selectedAnimationIndex != lastSelectedIndex)
			{
				player = new SimpleAnimationPlayer(animation);
				player.IsPlaying = autoPlayPref.Value;
				texturePtr = new IntPtr(spriteAtlas.Texture.Id);
				lastSelectedIndex = selectedAnimationIndex;
			}

			DrawControls(animation);
			DrawAnimation(spriteAtlas, animation);
		}

		ImGui.End();
	}

	private static void DrawControls(Animation animation)
	{
		bool autoPlay = autoPlayPref.Value;
		if (ImGui.Checkbox("Auto Play", ref autoPlay))
		{
			autoPlayPref.Value = autoPlay;
			player!.IsPlaying = autoPlay;
		}

		IntPtr icon = player!.IsPlaying ? new IntPtr(pauseIcon.Id) : new IntPtr(playIcon.Id);
		if (ImGui.ImageButton("PlayPauseButton", icon, playIcon.Size()))
			player.IsPlaying = !player.IsPlaying;

		ImGui.SameLine();

		float speed = player.Speed;
		if (ImGui.DragFloat("Speed", ref speed, v_speed: 0.1f, float.MinValue, float.MaxValue))
			player.Speed = speed;

		ImGui.BeginDisabled(player.IsPlaying || animation.FrameCount == 0);
		int frameIndex = player.FrameIndex;
		if (ImGui.SliderInt("Frame", ref frameIndex, animation.FirstFrameIndex, animation.LastFrameIndex))
			player.FrameIndex = frameIndex;

		float normalizedProgress = player.NormalizedTime;
		if (ImGui.SliderFloat("Cycle Progress", ref normalizedProgress, 0f, 1f))
			player.NormalizedTime = normalizedProgress;

		ImGui.EndDisabled();
	}

	private static void DrawAnimation(SpriteAtlas spriteAtlas, Animation animation)
	{
		if (animation.FrameCount == 0)
			return;

		player!.Update(Raylib.GetFrameTime());

		int tileId = animation.FrameAt(player.FrameIndex).TileId;
		spriteAtlas.GetRenderInfo(tileId, out Vector2 size, out Vector2 uv0, out Vector2 uv1);
		size = FitToWindow(size);
		ImGui.Image(texturePtr, size, uv0, uv1);
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