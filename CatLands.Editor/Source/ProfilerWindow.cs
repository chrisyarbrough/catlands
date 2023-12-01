namespace CatLands.Editor;

using System.Globalization;
using ImGuiNET;

public class ProfilerWindow : Window
{
	private static readonly Dictionary<string, TimeSpan> stats = new();

	public ProfilerWindow() : base("Profiler")
	{
	}

	protected override void DrawContent()
	{
		if (Profiler.Enabled)
		{
			foreach ((string name, TimeSpan elapsed) in stats)
			{
				ImGui.LabelText(elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture), name);
			}
		}
		else
		{
			ImGui.TextWrapped($"Define the symbol #{Profiler.ConditionString} to enable profiling.");
		}
	}

	public static void UpdateStats(IEnumerable<KeyValuePair<string, TimeSpan>> stats)
	{
		foreach ((string name, TimeSpan elapsed) in stats)
		{
			if (ProfilerWindow.stats.TryGetValue(name, out TimeSpan existingValue))
			{
				// Approximate a running average to smooth out the values for easier reading.
				long ticks = existingValue.Ticks;
				const int samples = 120;
				ticks -= ticks / samples;
				ticks += elapsed.Ticks / samples;

				ProfilerWindow.stats[name] = new TimeSpan(ticks);
			}
			else
			{
				ProfilerWindow.stats.Add(name, elapsed);
			}
		}
	}
}