namespace CatLands;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class JsonSerializer : IStreamSerializer
{
	public string FileExtension => ".json";
	
	private static readonly JsonSerializerSettings settings;

	static JsonSerializer()
	{
		settings = new JsonSerializerSettings
		{
			ContractResolver = new DefaultContractResolver()
			{
				NamingStrategy = new LowercaseNamingStrategy(),
			},
			Formatting = Formatting.Indented,
		};
	}

	public void WriteTo(object value, Stream stream)
	{
		using var writer = new StreamWriter(stream, leaveOpen: true);
		new Newtonsoft.Json.JsonSerializer().Serialize(writer, value);
	}

	public string Serialize(object value)
	{
		return JsonConvert.SerializeObject(value, settings);
	}

	public T? Deserialize<T>(string json)
	{
		return JsonConvert.DeserializeObject<T>(json, settings);
	}

	public T? ReadFrom<T>(Stream stream)
	{
		using var reader = new StreamReader(stream, leaveOpen: true);
		string json = reader.ReadToEnd();
		return Deserialize<T>(json);
	}

	private class LowercaseNamingStrategy : NamingStrategy
	{
		protected override string ResolvePropertyName(string name)
		{
			return name.ToLower();
		}
	}
}