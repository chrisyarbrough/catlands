namespace CatLands.Editor;

using Newtonsoft.Json;

// TODO: Turn into object/value classes.
public static class Prefs
{
	private static readonly FileInfo file = new(Path.Combine(AppContext.BaseDirectory, "Prefs.json"));
	private static readonly Dictionary<string, string> prefs = new();

	public static void Set(string key, string value)
	{
		prefs[key] = value;
		Save();
	}

	public static void Set(string key, int value)
	{
		prefs[key] = value.ToString();
		Save();
	}

	public static void Remove(string key)
	{
		prefs.Remove(key);
		Save();
	}

	private static void Save()
	{
		string json = JsonConvert.SerializeObject(prefs, Formatting.Indented);
		File.WriteAllText(file.FullName, json);
	}

	public static void Load()
	{
		if (!file.Exists)
			return;

		string json = File.ReadAllText(file.FullName);
		prefs.Clear();
		var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
		foreach (KeyValuePair<string, string> pair in data!)
			prefs[pair.Key] = pair.Value;
	}

	public static bool TryGet(string key, out string value)
	{
		return prefs.TryGetValue(key, out value!);
	}

	public static bool TryGet(string key, out int value)
	{
		value = default;
		return prefs.TryGetValue(key, out string? stringValue) && int.TryParse(stringValue, out value);
	}

	public static int Get(string key, int defaultValue)
	{
		if (TryGet(key, out int value))
			return value;
		return defaultValue;
	}

	public static string Get(string key, string defaultValue)
	{
		if (TryGet(key, out string value))
			return value;
		return defaultValue;
	}
}