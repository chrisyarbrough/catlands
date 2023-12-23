namespace CatLands;

using System.Numerics;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.TypeInspectors;

public class YamlSerializer : IStreamSerializer
{
	public string FileExtension => ".yaml";

	private static readonly INamingConvention namingConvention = PascalCaseNamingConvention.Instance;

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

	private static ISerializer CreateSerializer()
	{
		var serializer = new SerializerBuilder()
			.WithNamingConvention(namingConvention)
			.WithAnnotatedTagMappings()
			.WithAnnotatedTypeConverters()
			.EnsureRoundtrip()
			.DisableAliases()
			.WithTypeInspector<ITypeInspector>(_ => new CustomTypeInspector())
			.Build();
		return serializer;
	}

	private static IDeserializer CreateDeserializer()
	{
		var deserializer = new DeserializerBuilder()
			.WithNamingConvention(namingConvention)
			.WithAnnotatedTagMappings()
			.WithAnnotatedTypeConverters()
			.IgnoreUnmatchedProperties()
			.WithTypeInspector<ITypeInspector>(_ => new CustomTypeInspector())
			.Build();
		return deserializer;
	}
}