namespace CatLands;

public class AnimationPlayer : IAnimationPlayer
{
	private float time;

	private readonly Animation animation;

	public AnimationPlayer(Animation animation)
	{
		this.animation = animation;
		Reset();
	}

	public void Update(float deltaTime)
	{
		if (IsPlaying == false)
			return;

		time += deltaTime * Speed;

		if (time >= animation.Duration)
			time -= animation.Duration;
		else if (time < 0)
			time += animation.Duration;
	}

	public void Reset() => time = 0f;
	public void Play() => IsPlaying = true;
	public void Pause() => IsPlaying = false;

	public void Stop()
	{
		Pause();
		Reset();
	}

	public float Speed { get; set; } = 1f;

	public bool IsPlaying { get; private set; } = true;

	/// <summary>
	/// The animation time in seconds within one cycle.
	/// </summary>
	public float Time => time;

	/// <summary>
	/// The animation time within one cycle in the range 0 to 1.
	/// </summary>
	public float NormalizedTime
	{
		get => time / animation.Duration;
		set => time = value * animation.Duration;
	}

	public virtual int FrameIndex
	{
		get
		{
			float sampledTime = 0f;

			for (int frameIndex = 0; frameIndex < animation.FrameCount; frameIndex++)
			{
				sampledTime += animation.FrameDurationAt(frameIndex);

				if (sampledTime > time)
					return frameIndex;
			}

			if (sampledTime >= time)
				return animation.FrameCount - 1;

			throw new Exception("Failed to sample animation at time: " + time);
		}
		set
		{
			if (value < 0 || value > animation.FrameCount)
			{
				throw new ArgumentOutOfRangeException(nameof(value),
					$"Frame {value} is out of range. Must be greater 0 and less than {animation.FrameCount}.");
			}

			time = 0f;

			for (int frameIndex = 0; frameIndex < value; frameIndex++)
				time += animation.FrameDurationAt(frameIndex);
		}
	}
}