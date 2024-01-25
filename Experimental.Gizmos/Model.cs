namespace Experimental.Gizmos;

using YamlDotNet.Serialization;

public class Model
{
	public Dictionary<int, Rect> Rects { get; set; } = new();

	private int FindNextFreeId()
	{
		return Rects.Keys.Prepend(0).Max() + 1;
	}

	public int Add(Rect item)
	{
		int id = FindNextFreeId();
		Rects.Add(id, item);
		return id;
	}

	private const string savePath = "model.yaml";

	public static Model Load()
	{
		try
		{
			string yaml = File.ReadAllText(savePath);
			Model model = Deserialize(yaml);
			model.Rects ??= new();

			return model;
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			return new Model();
		}
	}

	public string Save()
	{
		string yaml = Serialize();
		File.WriteAllText(savePath, yaml);
		return yaml;
	}

	public static Model Deserialize(string yaml)
	{
		var deserializer = new DeserializerBuilder().Build();
		return deserializer.Deserialize<Model>(yaml);
	}

	public string Serialize()
	{
		var serializer = new SerializerBuilder()
			.EnsureRoundtrip()
			.Build();
		return serializer.Serialize(this);
	}
}