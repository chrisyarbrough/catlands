namespace CatLands;

public class Map
{
	public static event Action? CurrentChanged;

	public static Map? Current
	{
		get => current;
		set
		{
			if (value != current)
			{
				current = value;
				CurrentChanged?.Invoke();
			}
		}
	}

	private static Map? current;

	public int LayerCount => layers.Count;
	
	public string? FilePath { get; set; }

	public readonly ChangeTracker ChangeTracker = new();
	public IEnumerable<Layer> Layers => layers;

	private List<Layer> layers = new();

	public Map()
	{
	}

	public Map(IEnumerable<Layer> layers)
	{
		this.layers.AddRange(layers);
	}

	public Layer GetLayer(int index)
	{
		Layer layer = layers[index];
		layer.SetChangeTracker(ChangeTracker);
		return layer;
	}

	public int AddLayer(Layer layer)
	{
		int id = layers.Count;
		layers.Add(layer);
		layer.SetChangeTracker(ChangeTracker);
		ChangeTracker.NotifyChange();
		return id;
	}

	public void RemoveLayer(int index)
	{
		layers.RemoveAt(index);
		ChangeTracker.NotifyChange();
	}
}