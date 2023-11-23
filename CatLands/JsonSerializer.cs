using Newtonsoft.Json;

namespace CatLands;

using Newtonsoft.Json.Serialization;
using System.Reflection;

public class JsonSerializer : ISerializer
{
	private static readonly JsonSerializerSettings settings;

	static JsonSerializer()
	{
		settings = new JsonSerializerSettings
		{
			ContractResolver = new OnlyFieldsContractResolver(),
			Formatting = Formatting.Indented
		};
	}

	public void Serialize(Map map, Stream stream)
	{
		using var writer = new StreamWriter(stream, leaveOpen: true);
		string json = JsonConvert.SerializeObject(map, settings);
		writer.Write(json);
	}

	public Map? Deserialize(Stream stream)
	{
		using var reader = new StreamReader(stream, leaveOpen: true);
		string json = reader.ReadToEnd();
		return JsonConvert.DeserializeObject<Map>(json, settings);
	}

	private class OnlyFieldsContractResolver : DefaultContractResolver
	{
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			// Get all public and non-public properties and fields
			var memberInfos = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(m => m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field);

			var jsonProperties = new List<JsonProperty>();

			foreach (var memberInfo in memberInfos)
			{
				var attribute = memberInfo.GetCustomAttribute<SerializeMemberAttribute>();
				if (attribute != null)
				{
					var jsonProperty = base.CreateProperty(memberInfo, memberSerialization);

					if (memberInfo is PropertyInfo propertyInfo)
					{
						jsonProperty.Writable = propertyInfo.CanWrite;
						jsonProperty.Readable = propertyInfo.CanRead;
					}
					else if (memberInfo is FieldInfo)
					{
						jsonProperty.Writable = true;
						jsonProperty.Readable = true;
					}

					jsonProperty.PropertyName = memberInfo.Name;
					jsonProperty.ShouldSerialize = _ => true;
					jsonProperties.Add(jsonProperty);
				}
			}
			return jsonProperties;
		}
	}
}