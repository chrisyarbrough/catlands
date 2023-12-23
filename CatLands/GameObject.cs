namespace CatLands;

using Raylib_cs;

public class GameObject
{
	public string Name
	{
		get
		{
			if (name == null)
				return GetType().Name;

			return name;
		}
		set => name = value;
	}

	private string? name;

	private readonly List<GameObject> children = new();

	public GameObject()
	{
	}

	public GameObject(string name)
	{
		this.name = name;
	}

	public virtual void Setup()
	{
		foreach (GameObject child in children)
			child.Setup();
	}

	public virtual void Shutdown()
	{
		foreach (GameObject child in children)
			child.Shutdown();
	}

	public virtual void Update()
	{
		foreach (GameObject child in children)
			child.Update();
	}

	/// <summary>
	/// An editor callback invoked by each SceneView when it renders.
	/// </summary>
	public virtual void OnSceneGui(Camera2D camera)
	{
		foreach (GameObject child in children)
			child.OnSceneGui(camera);
	}

	public void AddChild(GameObject gameObject) => children.Add(gameObject);

	public IEnumerable<GameObject> Children => children;

	public bool HasChildren => Children.Any();
}