namespace CatLands;

public static class AssetFile
{
	public static IEnumerable<string> SupportedFileExtensions => serializers.Select(s => s.FileExtension);

	private static readonly List<ISerializer> serializers = new()
	{
		new JsonSerializer(),
		new YamlSerializer()
		//new BinarySerializer(),
	};

	private static bool TryGetSerializer(string filePath, out ISerializer serializer)
	{
		foreach (ISerializer s in serializers)
		{
			if (filePath.EndsWith(s.FileExtension, StringComparison.OrdinalIgnoreCase))
			{
				serializer = s;
				return true;
			}
		}

		serializer = default!;
		return false;
	}

	public static T? Load<T>(string filePath)
	{
		if (TryGetSerializer(filePath, out ISerializer serializer))
		{
			using FileStream stream = File.OpenRead(filePath);
			return serializer.Deserialize<T>(stream);
		}

		return default;
	}

	public static void Save<T>(string filePath, T value)
	{
		if (value == null)
			throw new ArgumentNullException(nameof(value));

		if (TryGetSerializer(filePath, out ISerializer serializer))
		{
			// Overwrite existing files. OpenWrite would not truncate a longer file.
			using FileStream stream = File.Create(filePath);
			serializer.Serialize(value, stream);
		}
		else
		{
			throw new Exception("Failed to save to: " + filePath);
		}
	}
}