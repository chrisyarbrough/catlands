using Newtonsoft.Json;

namespace CatLands;

public class JsonSerializer : ISerializer
{
	public void Serialize(Map map, Stream stream)
	{
		using var writer = new StreamWriter(stream, leaveOpen: true);
		string json = JsonConvert.SerializeObject(map, Formatting.Indented);
		writer.Write(json);
	}

	public Map? Deserialize(Stream stream)
	{
		using var reader = new StreamReader(stream, leaveOpen: true);
		string json = reader.ReadToEnd();
		return JsonConvert.DeserializeObject<Map>(json);
	}
}