namespace CatLands.Tests;

using System.Numerics;
using Xunit.Abstractions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using File = System.IO.File;
using ISerializer = CatLands.ISerializer;
using Color = System.Drawing.Color;

public class SerializerTests
{
	private readonly ITestOutputHelper testOutputHelper;

	private readonly Map sampleMap = new(new Layer[]
	{
		// new Layer("FakeTextureId_0"),
		// new Layer("FakeTextureId_1"),
		new Layer("FakeTextureId_2",
			new[] { (new Coord(1, 2), 3) }),
	});

	public SerializerTests(ITestOutputHelper testOutputHelper)
	{
		this.testOutputHelper = testOutputHelper;
	}

	[Fact]
	public void JsonSerializeDoesntThrow()
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
		// using var stream = new MemoryStream();
		// serializer.Serialize(sampleMap, stream);
		// stream.Seek(0, SeekOrigin.Begin);
		// Map? deserializedMap = serializer.Deserialize(stream);
		//
		// Assert.NotNull(deserializedMap);
		//
		// deserializedMap.Should().BeEquivalentTo(sampleMap, options => options
		// 	.ComparingByMembers<Map>()
		// 	.WithStrictOrdering());
	}

	[SerializeDerivedTypes]
	private abstract class Component
	{
		public List<float> someFloats { get; set; } = new() { 1.5f, 2, 3 };
	}

	private class ChildComponentA : Component
	{
		public string PropertyA { get; set; } = "42";

		public Vector2 MyVector { get; set; } = new(1, 2);
		//public Coord MyCoord { get; set; } = new(3, 4);
	}

	private class ChildComponentB : Component
	{
		public float PropertyB { get; set; } = 1.2f;
		public string myName { get; set; } = "B";
		public MyCoord MyCoord { get; set; } = new() { X = 3, Y = 4 };
		public System.Drawing.Color color { get; set; } = Color.Green;
		public List<Color> colors { get; set; } = new() { Color.Red, Color.Blue };
	}

	public class Bla
	{
		public int X { get; set; }
		public int Y { get; set; }
	}

	public class MyCoord
	{
		public int X;
		public int Y;
	}


	[YamlTypeConverter("Instance")]
	public class ColorConverter : IYamlTypeConverter
	{
		public static readonly ColorConverter Instance = new();

		public bool Accepts(Type type)
		{
			return type == typeof(System.Drawing.Color);
		}

		public object ReadYaml(IParser parser, Type type)
		{
			parser.Consume<MappingStart>();

			parser.Consume<Scalar>();
			byte r = byte.Parse(parser.Consume<Scalar>().Value);

			parser.Consume<Scalar>();
			byte g = byte.Parse(parser.Consume<Scalar>().Value);

			parser.Consume<Scalar>();
			byte b = byte.Parse(parser.Consume<Scalar>().Value);

			parser.Consume<MappingEnd>();
			return Color.FromArgb(r, g, b);
		}

		public void WriteYaml(IEmitter emitter, object? value, Type type)
		{
			emitter.Emit(new MappingStart());

			Color color = (Color)value!;

			emitter.Emit(new Scalar(nameof(color.R)));
			emitter.Emit(new Scalar(color.R.ToString()));

			emitter.Emit(new Scalar(nameof(color.G)));
			emitter.Emit(new Scalar(color.G.ToString()));

			emitter.Emit(new Scalar(nameof(color.B)));
			emitter.Emit(new Scalar(color.B.ToString()));

			emitter.Emit(new MappingEnd());
		}
	}

	[Fact]
	public void YamlCanHandlePolymorphism()
	{
		var a = new ChildComponentA();
		List<Component> components = new()
		{
			a,
			a,
			new ChildComponentB(),
		};

		var serializer = new SerializerBuilder()
			.WithAnnotatedTagMappings()
			.WithAnnotatedTypeConverters()
			.EnsureRoundtrip()
			.Build();
		string yaml = serializer.Serialize(components);
		testOutputHelper.WriteLine(yaml);
		
		var deserializer = new DeserializerBuilder()
			.WithAnnotatedTagMappings()
			.WithAnnotatedTypeConverters()
			.Build();
		var deserializedComponents = deserializer.Deserialize<List<Component>>(yaml);
		testOutputHelper.WriteLine("Deserialized:\n" +
		                           string.Join("\n", deserializedComponents.Select(c => c.GetType().Name)));
		Assert.Same(deserializedComponents[0], deserializedComponents[1]);
	}
}