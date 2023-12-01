namespace CatLands.Editor;

using System.Diagnostics;

public static class Profiler
{
	public const string ConditionString = "PROFILING";

	private static readonly Stack<ProfilerSample> samples = new();
	private static readonly Dictionary<string, TimeSpan> statistics = new();

	public static bool Enabled
	{
		get
		{
#if conditional
			return true;
#else
			return false;
#endif
		}
	}

	[Conditional(ConditionString)]
	public static void BeginFrame()
	{
		ProfilerWindow.UpdateStats(statistics.ToArray());
		statistics.Clear();
		samples.Clear();
	}

	[Conditional(ConditionString)]
	public static void BeginSample(string name)
	{
		samples.Push(new ProfilerSample(name));
	}

	[Conditional(ConditionString)]
	public static void EndSample()
	{
		ProfilerSample sample = samples.Pop();
		sample.Stop();
		statistics[sample.Name] = sample.Elapsed;
	}

	private readonly struct ProfilerSample
	{
		public readonly string Name;
		public TimeSpan Elapsed => watch.Elapsed;

		private readonly Stopwatch watch;

		public ProfilerSample(string name)
		{
			Name = name;
			watch = Stopwatch.StartNew();
		}

		public void Stop()
		{
			watch.Stop();
		}
	}
}