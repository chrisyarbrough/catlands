namespace CatLands;

public class OptimizedAnimationPlayer : AnimationPlayer
{
	private float[] cumulativeFrameDurations;

	public OptimizedAnimationPlayer(Animation animation) : base(animation)
	{
		// Precompute the time at which each frame ends.
		cumulativeFrameDurations = new float[animation.FrameCount];
		float totalDuration = 0f;
		for (int i = 0; i < animation.Frames.Count; i++)
		{
			totalDuration += animation.Frames[i].Duration;
			cumulativeFrameDurations[i] = totalDuration;
		}
	}

	public override int FrameIndex
	{
		get
		{
			// Binary search to quickly find the matching duration.
			int left = 0;
			int right = cumulativeFrameDurations.Length - 1;

			while (left <= right)
			{
				int mid = left + (right - left) / 2;
				if (cumulativeFrameDurations[mid] <= Time)
					left = mid + 1;
				else if (mid > 0 && cumulativeFrameDurations[mid - 1] > Time)
					right = mid - 1;
				else
					return mid;
			}

			if (left > right)
				return right;

			throw new Exception("Failed to sample animation at time: " + Time);
		}
		set => base.FrameIndex = value;
	}
}