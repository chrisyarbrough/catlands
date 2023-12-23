namespace CatLands.Editor;

using Newtonsoft.Json;

public static class Prefs
{
	public static ITextStorage Storage { get; set; } =
		new FileStorage(Path.Combine(AppContext.BaseDirectory, "Prefs.json"));

	public static IEnumerable<(string key, string value)> All => prefs.Select(pair => (pair.Key, pair.Value));

	private static readonly Dictionary<string, string> prefs = new();

	public static void Set(string key, object value)
	{
		prefs[key] = Serialize(value);
		Save();
	}

	public static bool TryGet<T>(string key, out T? value)
	{
		if (prefs.TryGetValue(key, out string? json))
		{
			try
			{
				value = Deserialize<T>(json);
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		value = default;
		return false;
	}


	public static T? Get<T>(string key, T? defaultValue = default)
	{
		if (TryGet(key, out T? value))
			return value;

		return defaultValue;
	}

	public static void Remove(string key)
	{
		prefs.Remove(key);
		Save();
	}

	private static void Save()
	{
		string json = Serialize(prefs);
		Storage.WriteAllText(json);
	}

	public static void Load()
	{
		prefs.Clear();

		if (!Storage.Exists)
			return;

		string json = Storage.ReadAllText();
		var data = Deserialize<Dictionary<string, string>>(json);

		if (data == null)
			return;

		foreach (KeyValuePair<string, string> pair in data)
			prefs[pair.Key] = pair.Value;
	}

	private static string Serialize<T>(T value) => JsonConvert.SerializeObject(value, Formatting.Indented);

	private static T? Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json);
}