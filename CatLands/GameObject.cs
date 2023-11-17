namespace CatLands;

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

	public virtual void Update()
	{
	}

	public virtual void OnGui(bool isActive)
	{
	}

	private readonly List<GameObject> children = new List<GameObject>();

	public GameObject()
	{
	}

	public GameObject(string name)
	{
		this.name = name;
	}

	public void AddChild(GameObject gameObject) => children.Add(gameObject);

	public IEnumerable<GameObject> Children => children;

	public bool HasChildren => Children.Any();
}