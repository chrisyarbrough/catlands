namespace CatLands;

public class SimpleAnimationPlayer : IAnimationPlayer
{
	private float elapsedTimeInFrame;

	private readonly Animation animation;

	public bool IsPlaying { get; set; } = true;

	public float Time
	{
		get
		{
			float time = 0f;
			for (int i = 0; i < FrameIndex; i++)
				time += animation.FrameDurationAt(i);
			return time + elapsedTimeInFrame;
		}
	}

	public float NormalizedTime
	{
		get
		{
			if (animation.FrameCount == 0)
				return 0f;

			return Time / animation.Duration;
		}
		set
		{
			float elapsedTime = Math.Clamp(value, 0.0f, 1.0f) * animation.Duration;

			float accumulatedDuration = 0.0f;
			for (int i = 0; i < animation.Frames.Count; i++)
			{
				accumulatedDuration += animation.FrameDurationAt(i);
				if (elapsedTime <= accumulatedDuration)
				{
					FrameIndex = i;
					elapsedTimeInFrame = elapsedTime - (accumulatedDuration - animation.FrameDurationAt(i));
					break;
				}
			}
		}
	}

	public int FrameIndex { get; set; }

	public float Speed { get; set; } = 1f;

	public SimpleAnimationPlayer(Animation animation)
	{
		this.animation = animation;
		Reset();
	}

	public void Update(float deltaTime)
	{
		if (animation.FrameCount == 0)
		{
			FrameIndex = -1;
			return;
		}

		if (IsPlaying == false)
			return;

		Animation.Frame currentFrame = animation.Frames[FrameIndex];

		elapsedTimeInFrame += deltaTime * Math.Abs(Speed);

		// Handle large deltaTime or speed values by capping the frame duration.
		elapsedTimeInFrame = Math.Min(elapsedTimeInFrame, currentFrame.Duration);

		if (elapsedTimeInFrame >= currentFrame.Duration)
		{
			elapsedTimeInFrame -= currentFrame.Duration;

			FrameIndex += Math.Sign(Speed);

			if (FrameIndex < 0)
				FrameIndex = animation.Frames.Count - 1;

			else if (this.FrameIndex >= animation.Frames.Count)
				FrameIndex = 0;
		}
	}

	public void Reset()
	{
		FrameIndex = 0;
		elapsedTimeInFrame = 0f;
	}

	public void Play() => IsPlaying = true;

	public void Pause() => IsPlaying = false;


	public void Stop()
	{
		Pause();
		Reset();
	}
}