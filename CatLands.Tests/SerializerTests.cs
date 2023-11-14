namespace CatLands.Tests;

public class SerializerTests
{
	private readonly Map sampleMap = new()
	{
		Version = 42,
		Tilesets = new[] { "MyTileset.png" },
		Tiles = new List<Tile>
		{
			new Tile { Id = 0, Coord = new Coord(0, 0) },
			new Tile { Id = 1, Coord = new Coord(1, 0) },
			new Tile { Id = 2, Coord = new Coord(1, 0) },
			new Tile { Id = 3, Coord = new Coord(1, 1) },
		}
	};

	public static IEnumerable<object[]> Serializers()
	{
		yield return new object[] { new JsonSerializer() };
		yield return new object[] { new BinarySerializer() };
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