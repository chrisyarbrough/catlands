using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using CatLands;

BenchmarkRunner.Run<AnimationPlayerBenchmark>();

// Resharper disable all

public class AnimationPlayerBenchmark
{
	[Params(10, 100, 500)]
	public int FrameCount { get; set; }

	[Params(0f, 0.5f, 1f)]
	public float NormalizedTime { get; set; }

	private AnimationPlayer animationPlayer;
	private OptimizedAnimationPlayer optimizedAnimationPlayer;
	private SimpleAnimationPlayer simpleAnimationPlayer;

	[GlobalSetup]
	public void Setup()
	{
		var animation = new Animation();
		for (int i = 0; i < FrameCount; i++)
		{
			animation.Frames.Add(new Animation.Frame(i, duration: 0.25f));
		}

		animationPlayer = new AnimationPlayer(animation);
		animationPlayer.NormalizedTime = NormalizedTime;

		optimizedAnimationPlayer = new OptimizedAnimationPlayer(animation);
		optimizedAnimationPlayer.NormalizedTime = NormalizedTime;

		simpleAnimationPlayer = new SimpleAnimationPlayer(animation);
		simpleAnimationPlayer.NormalizedTime = NormalizedTime;
	}

	[Benchmark]
	public void AnimationPlayer()
	{
		animationPlayer.Update(0f);
		int i = animationPlayer.FrameIndex;
	}

	[Benchmark]
	public void OptimizedAnimationPlayer()
	{
		optimizedAnimationPlayer.Update(0f);
		int i = optimizedAnimationPlayer.FrameIndex;
	}

	[Benchmark]
	public void SimpleAnimaitonPlayer()
	{
		simpleAnimationPlayer.Update(0f);
		int i = simpleAnimationPlayer.FrameIndex;
	}
}