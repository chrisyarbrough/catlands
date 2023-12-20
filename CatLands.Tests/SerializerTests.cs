namespace CatLands.Tests;

using File = System.IO.File;

public class SerializerTests
{
	private readonly Map sampleMap = new(new Layer[]
	{
		// new Layer("FakeTextureId_0"),
		// new Layer("FakeTextureId_1"),
		new Layer("FakeTextureId_2",
			new[] { (new Coord(1, 2), 3) }),
	});

	[Fact]
	public void Bla()
	{
		var stream = File.Create("File.json");
		new JsonSerializer().Serialize(sampleMap, stream);
	}

	public static IEnumerable<object[]> Serializers()
	{
		yield return new object[] { new JsonSerializer() };
		// yield return new object[] { new BinarySerializer() };
	}

	[Theory]
	[MemberData(nameof(Serializers))]
	public void MapRoundTripWorks(ISerializer serializer)
	{
		using var stream = new MemoryStream();
		serializer.Serialize(sampleMap, stream);
		stream.Seek(0, SeekOrigin.Begin);
		Map? deserializedMap = serializer.Deserialize(stream);

		Assert.NotNull(deserializedMap);

		deserializedMap.Should().BeEquivalentTo(sampleMap, options => options
			.ComparingByMembers<Map>()
			.WithStrictOrdering());
	}
}