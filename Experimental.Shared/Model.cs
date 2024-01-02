using System.Numerics;
using YamlDotNet.Serialization;

public struct Offset
{
	public float X;
	public float Y;

	public static explicit operator Offset(Vector2 v) => new() { X = v.X, Y = v.Y };
}

// A pixel rectangle.
public struct Rect
{
	public float X { get; set; }

	public float Y { get; set; }

	public float Width { get; set; }

	public float Height { get; set; }

	public Rect(float x, float y, float width, float height)
	{
		X = (int)x;
		Y = (int)y;
		Width = (int)width;
		Height = (int)height;
	}

	public static explicit operator Raylib_cs.Rectangle(Rect r) => new(r.X, r.Y, r.Width, r.Height);
}

public class Model
{
	public Dictionary<int, Rect> Items { get; set; } = new();

	private int FindNextFreeId()
	{
		int id = 0;

		foreach (int i in Items.Keys)
			id = Math.Max(i, id);

		return id + 1;
	}

	public int Add(Rect item)
	{
		int id = FindNextFreeId();
		Items.Add(id, item);
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