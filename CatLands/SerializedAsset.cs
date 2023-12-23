namespace CatLands;

public sealed class SerializedAsset
{
	#region Static

	/// <summary>
	/// The extensions of supported file types including the period (".") character.
	/// </summary>
	public static IEnumerable<string> SupportedFileExtensions => serializers.Keys;

	private static readonly Dictionary<string, IStreamSerializer> serializers = new()
	{
		{ ".json", new JsonSerializer() },
		{ ".yaml", new YamlSerializer() },
		//{ ".bin", new BinarySerializer() },
	};

	private static bool TryGetSerializer(string filePath, out IStreamSerializer serializer)
	{
		return serializers.TryGetValue(Path.GetExtension(filePath).ToLower(), out serializer!);
	}

	public static T? Load<T>(string path)
	{
		if (TryGetSerializer(path, out IStreamSerializer serializer))
		{
			using FileStream stream = File.OpenRead(path);
			return serializer.ReadFrom<T>(stream);
		}

		return default;
	}

	public static T? Load<T>(string path, out SerializedAsset handle)
	{
		if (TryGetSerializer(path, out IStreamSerializer serializer))
		{
			using FileStream stream = File.OpenRead(path);
			T value = serializer.ReadFrom<T>(stream)!;
			handle = new SerializedAsset(path, value);
			return value;
		}

		handle = null!;
		return default;
	}

	public static void Save(string path, object value)
	{
		if (value == null)
			throw new ArgumentNullException(nameof(value));

		if (TryGetSerializer(path, out IStreamSerializer serializer))
		{
			// Overwrite existing files. OpenWrite would not truncate a longer file.
			using FileStream stream = File.Create(path);
			serializer.WriteTo(value, stream);
		}
		else
		{
			throw new Exception("Failed to save to: " + path);
		}
	}

	#endregion

	private readonly string path;
	private readonly object value;

	public SerializedAsset(string path, object value)
	{
		this.path = path;
		this.value = value;
	}

	public void Save()
	{
		Save(path, value);
	}
}