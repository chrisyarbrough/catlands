namespace CatLands.Editor;

using Newtonsoft.Json;

public static class Prefs
{
	private static readonly FileInfo file = new(Path.Combine(AppContext.BaseDirectory, "Prefs.json"));
	private static readonly Dictionary<string, object> prefs = new();

	public static void Set(string key, object value)
	{
		prefs[key] = value;
		Save();
	}

	public static bool TryGet<T>(string key, out T? value)
	{
		if (prefs.TryGetValue(key, out object? obj))
		{
			try
			{
				value = (T)obj;
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
}