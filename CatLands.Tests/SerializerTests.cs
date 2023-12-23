namespace CatLands.Tests;

using System.Text;
using Xunit.Abstractions;

public class SerializerTests
{
	private readonly ITestOutputHelper testOutputHelper;

	public SerializerTests(ITestOutputHelper testOutputHelper)
	{
		this.testOutputHelper = testOutputHelper;
	}

	public static IEnumerable<object[]> Serializers()
	{
		//yield return new object[] { new JsonSerializer() };
		//yield return new object[] { new BinarySerializer() };
		yield return new object[] { new YamlSerializer() };
	}

	[Theory]
	[MemberData(nameof(Serializers))]
	public void RoundtripWorks(IStreamSerializer serializer)
	{
		var sourceObject = new SampleObject();

		using var stream = new MemoryStream();
		serializer.WriteTo(sourceObject, stream);
		stream.Seek(0, SeekOrigin.Begin);

		SampleObject? deserializedObject = serializer.ReadFrom<SampleObject>(stream);
		try
		{
			Assert.NotNull(deserializedObject);

			// This compares only public members.
			deserializedObject.Should().BeEquivalentTo(sourceObject, options => options
				.ComparingByMembers<Map>()
				.WithStrictOrdering());
		}
		finally
		{
			string text = Encoding.UTF8.GetString(stream.ToArray());
			testOutputHelper.WriteLine("Serialized text:");
			testOutputHelper.WriteLine(text);
		}
	}

	[Theory]
	[MemberData(nameof(Serializers))]
	public void AccessModifiersWork(IStreamSerializer serializer)
	{
		var sourceObject = new AccessModifiersObject(4f);

		using var stream = new MemoryStream();
		serializer.WriteTo(sourceObject, stream);
		stream.Seek(0, SeekOrigin.Begin);

		var deserializedObject = serializer.ReadFrom<AccessModifiersObject>(stream);
		try
		{
			Assert.NotNull(deserializedObject);
			deserializedObject.AssertDeserializedCorrectly(sourceObject);
		}
		finally
		{
			string text = Encoding.UTF8.GetString(stream.ToArray());
			testOutputHelper.WriteLine("Serialized text:");
			testOutputHelper.WriteLine(text);
		}
	}
}