namespace CatLands;

public static class SaveFileManager
{
	private static readonly Dictionary<string, Func<ISerializer>> serializerLookup = new()
	{
		{ ".json", () => new JsonSerializer() },
		// { ".bin", () => new BinarySerializer() },
	};

	public static Map? Load(string filePath)
	{
		try
		{
			if (serializerLookup.TryGetValue(Path.GetExtension(filePath), out Func<ISerializer>? serializerFactory))
			{
				using FileStream stream = File.OpenRead(filePath);
				Map? map = serializerFactory().Deserialize(stream);
				if (map != null)
					map.FilePath = filePath;
				return map;
			}
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}

		return null;
	}

	public static void Save(string filePath, Map map)
	{
		if (serializerLookup.TryGetValue(Path.GetExtension(filePath), out Func<ISerializer>? serializerFactory))
		{
			// Overwrite existing files. OpenWrite would not truncate a longer file.
			using FileStream stream = File.Create(filePath);
			serializerFactory().Serialize(map, stream);
			map.FilePath = filePath;
		}
		else
		{
			throw new Exception("Failed to save to: " + filePath);
		}
	}
}