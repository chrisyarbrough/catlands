namespace CatLands;

public interface IAnimationPlayer
{
	bool IsPlaying { get; }

	/// <summary>
	/// The animation time in seconds within one cycle.
	/// </summary>
	float Time { get; }

	/// <summary>
	/// The animation time within one cycle in the range 0 to 1.
	/// </summary>
	float NormalizedTime { get; set; }

	int FrameIndex { get; set; }
	
	float Speed { get; set; }

	void Update(float deltaTime);
	void Reset();
	void Play();
	void Pause();
	void Stop();
}