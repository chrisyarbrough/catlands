namespace CatLands;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class YamlSerializer : ISerializer
{
	public string FileExtension => ".yaml";

	private static readonly INamingConvention namingConvention = CamelCaseNamingConvention.Instance;

	public void Serialize(object value, Stream stream)
	{
		string yaml = Serialize(value);
		ISerializer.Write(yaml, stream);
	}

	public T Deserialize<T>(Stream stream)
	{
		var deserializer = new DeserializerBuilder()
			.WithNamingConvention(namingConvention)
			.WithAnnotatedTagMappings()
			.WithAnnotatedTypeConverters()
			.IgnoreUnmatchedProperties()
			.Build();

		using var reader = new StreamReader(stream, leaveOpen: true);
		return deserializer.Deserialize<T>(reader);
	}

	public static string Serialize(object value)
	{
		var serializer = new SerializerBuilder()
			.WithNamingConvention(namingConvention)
			.WithAnnotatedTagMappings()
			.WithAnnotatedTypeConverters()
			.EnsureRoundtrip()
			.Build();

		return serializer.Serialize(value);
	}

	public static T Deserialize<T>(string yaml)
	{
		var deserializer = new DeserializerBuilder()
			.WithNamingConvention(namingConvention)
			.IgnoreUnmatchedProperties()
			.Build();

		return deserializer.Deserialize<T>(yaml);
	}
}