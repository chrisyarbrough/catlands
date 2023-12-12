namespace CatLands.SpriteEditor;

public class AnimationPlayer
{
	public float Speed = 1f;

	private Animation? animation;
	private int frameIndex;
	private float elapsedTimeInFrame;

	public void SetAnimation(Animation animation)
	{
		this.animation = animation;
		this.frameIndex = 0;
		this.elapsedTimeInFrame = 0f;
	}

	public bool Update(float deltaTime, out int frameIndex)
	{
		if (animation == null || animation.FrameCount == 0)
		{
			frameIndex = -1;
			return false;
		}

		Animation.Frame currentFrame = animation.Frames[this.frameIndex];

		elapsedTimeInFrame += deltaTime * Math.Abs(Speed);

		// Handle large deltaTime or speed values by capping the frame duration.
		elapsedTimeInFrame = Math.Min(elapsedTimeInFrame, currentFrame.Duration);

		if (elapsedTimeInFrame >= currentFrame.Duration)
		{
			elapsedTimeInFrame -= currentFrame.Duration;

			this.frameIndex += Math.Sign(Speed);

			if (this.frameIndex < 0)
				this.frameIndex = animation.Frames.Count - 1;

			else if (this.frameIndex >= animation.Frames.Count)
				this.frameIndex = 0;
		}

		frameIndex = this.frameIndex;
		return true;
	}
}