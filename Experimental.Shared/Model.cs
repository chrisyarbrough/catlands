using System.Numerics;
using Raylib_cs;
using YamlDotNet.Serialization;

public class Model
{
	public Dictionary<int, Rectangle> Items { get; set; } = new();

	public int Add(Vector2 position, float size)
	{
		int id = Items.Count == 0 ? 0 : Items.Keys.Max() + 1;
		Items.Add(id, new Rectangle(position.X, position.Y, size, size));
		return id;
	}
	
	private const string savePath = "model.yaml";

	public static Model Load()
	{
		try
		{
			string yaml = File.ReadAllText(savePath);
			Model model = Deserialize(yaml);
			model.Items ??= new();

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