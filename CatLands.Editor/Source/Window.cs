namespace CatLands.Editor;

using System.Numerics;
using ImGuiNET;
using Raylib_cs;

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

	/// <summary>
	/// If false, signal to the window management that this window should be destroyed and no longer updated.
	/// </summary>
	internal bool IsOpen = true;

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

	public virtual void Setup()
	{
	}

	public virtual void Shutdown()
	{
	}

	public virtual void Update()
	{
	}

	public void Draw()
	{
		ImGuiWindowFlags flags = SetupWindow();
		ImGui.SetNextWindowSize(new Vector2(500, 400), ImGuiCond.FirstUseEver);
		ImGui.SetNextWindowSizeConstraints(new Vector2(200, 200),
			new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight()));

		if (ImGui.Begin(Name, ref IsOpen, flags))
		{
			// Draw if not collapsed.
			DrawContent();
		}

		ImGui.End();
	}

	protected virtual ImGuiWindowFlags SetupWindow()
	{
		return ImGuiWindowFlags.None | ImGuiWindowFlags.NoCollapse;
	}

	protected virtual void DrawContent()
	{
	}

	public virtual void OnSceneGui()
	{
	}
}