namespace CatLands.Editor;

public abstract class Window
{
	public static IEnumerable<T> GetInstances<T>() where T : Window
	{
		if (windowsByType.TryGetValue(typeof(T), out List<Window>? windows) && windows != null)
		{
			foreach (Window window in windows)
			{
				if (window is T typedWindow)
					yield return typedWindow;
			}
		}
	}

	private static readonly Dictionary<Type, List<Window>?> windowsByType = new();

	public readonly string Name;

	protected Window(string name)
	{
		if (!windowsByType.TryGetValue(GetType(), out List<Window>? instances))
		{
			instances = new List<Window>();
			windowsByType.Add(GetType(), instances);
		}
		// ImGui identified windows by their name, so they have to be unique.
		// Conveniently, everything after the double pounds is hidden.
		Name = name + "##" + instances!.Count;
		instances.Add(this);
	}

	~Window()
	{
		if (windowsByType.TryGetValue(GetType(), out List<Window>? windows) && windows != null)
		{
			windows.Remove(this);
		}
	}

	public abstract void Render();
}