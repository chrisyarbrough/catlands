namespace CatLands;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class YamlSerializer : IStreamSerializer
{
	public string FileExtension => ".yaml";

	public void WriteTo(object value, Stream stream)
	{
		ISerializer serializer = CreateSerializer();
		using var writer = new StreamWriter(stream, leaveOpen: true);
		serializer.Serialize(writer, value);
	}

	public static string Serialize(object value)
	{
		ISerializer serializer = CreateSerializer();
		return serializer.Serialize(value);
	}

	public T ReadFrom<T>(Stream stream)
	{
		IDeserializer deserializer = CreateDeserializer();
		using var reader = new StreamReader(stream, leaveOpen: true);
		return deserializer.Deserialize<T>(input: reader);
	}

	public static T Deserialize<T>(string yaml)
	{
		IDeserializer deserializer = CreateDeserializer();
		return deserializer.Deserialize<T>(yaml);
	}

	private static ISerializer CreateSerializer() =>
		new SerializerBuilder()
			.WithAnnotatedTagMappings()
			.WithAnnotatedTypeConverters()
			.EnsureRoundtrip()
			.DisableAliases()
			.WithCustomTypeInspector()
			.Build();

	private static IDeserializer CreateDeserializer() =>
		new DeserializerBuilder()
			.WithAnnotatedTagMappings()
			.WithAnnotatedTypeConverters()
			.IgnoreUnmatchedProperties()
			.WithCustomTypeInspector()
			.Build();
}