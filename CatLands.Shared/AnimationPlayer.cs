namespace CatLands.SpriteEditor;

public class AnimationPlayer
{
	public float Speed = 1f;
	
	private Animation currentAnimation;
	private int currentFrameIndex;
	private float elapsedTimeInFrame;

	public AnimationPlayer(Animation animation)
	{
		currentAnimation = animation;
		currentFrameIndex = 0;
		elapsedTimeInFrame = 0.0f;
	}

	public int Update(float deltaTime)
	{
		if (currentAnimation.FrameCount == 0)
		{
			return -1;
		}

		Animation.Frame currentFrame = currentAnimation.Frames[currentFrameIndex];

		elapsedTimeInFrame += deltaTime * Math.Abs(Speed);

		// Handle large deltaTime or speed values by capping the frame duration.
		elapsedTimeInFrame = Math.Min(elapsedTimeInFrame, currentFrame.Duration);

		if (elapsedTimeInFrame >= currentFrame.Duration)
		{
			elapsedTimeInFrame -= currentFrame.Duration;

			currentFrameIndex += Math.Sign(Speed);

			if (currentFrameIndex < 0)
				currentFrameIndex = currentAnimation.Frames.Count - 1;

			else if (currentFrameIndex >= currentAnimation.Frames.Count)
				currentFrameIndex = 0;
		}

		return currentFrameIndex;
	}
}