namespace CatLands.SpriteEditor;

using System.Numerics;
using CatLands;
using Editor;
using ImGuiNET;
using Raylib_cs;

internal class AnimationPreviewWindow : Window
{
	private readonly AnimationEditor data;

	private static IAnimationPlayer? player;
	private static Animation? lastSelectedAnimation;
	private static Texture2D playIcon, pauseIcon;
	private static readonly Pref<bool> autoPlayPref = new("AutoPlay");

	public AnimationPreviewWindow(AnimationEditor data) : base("Animation Preview")
	{
		this.data = data;
	}

	public override void Load()
	{
		playIcon = Raylib.LoadTexture("play.png");
		pauseIcon = Raylib.LoadTexture("pause.png");
	}

	protected override void DrawContent()
	{
		if (data.TryGetSelectedAnimation(out Animation animation))
		{
			if (animation != lastSelectedAnimation)
			{
				player = new SimpleAnimationPlayer(animation);
				player.IsPlaying = autoPlayPref.Value;
				lastSelectedAnimation = animation;
			}

			DrawControls(animation);
			DrawAnimation(animation);
		}
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

	private void DrawAnimation(Animation animation)
	{
		if (animation.FrameCount == 0)
			return;

		player!.Update(Raylib.GetFrameTime());

		int tileId = animation.FrameAt(player.FrameIndex).TileId;
		if (data.SpriteAtlas.GetRenderInfo(tileId, out Vector2 size, out Vector2 uv0, out Vector2 uv1))
		{
			size = MathUtility.FitTo(size, ImGui.GetContentRegionAvail());
			ImGui.Image(data.TexturePointer, size, uv0, uv1);
		}
	}
}