namespace CatLands;

public static class SaveFileManager
{
	private static readonly Dictionary<string, Func<ISerializer>> serializerLookup = new()
	{
		{ ".json", () => new JsonSerializer() },
		{ ".bin", () => new BinarySerializer() },
	};

	public static Map? Load(string filePath)
	{
		if (serializerLookup.TryGetValue(Path.GetExtension(filePath), out Func<ISerializer>? serializerFactory))
		{
			using FileStream stream = File.OpenRead(filePath);
			return serializerFactory().Deserialize(stream);
		}

		throw new Exception("Failed to load from: " + filePath);
	}
}