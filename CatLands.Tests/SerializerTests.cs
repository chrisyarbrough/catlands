namespace CatLands.Tests;

using System.Numerics;
using System.Reflection;
using System.Text;
using Xunit.Abstractions;

public class SerializerTests
{
	private readonly ITestOutputHelper testOutputHelper;

	public SerializerTests(ITestOutputHelper testOutputHelper)
	{
		this.testOutputHelper = testOutputHelper;
	}

	[Fact]
	public void ManualYamlTest()
	{
		string p = "/Users/Chris/Projects/CatLands/test.yaml";
		using var file = File.Open(p, FileMode.OpenOrCreate);
		var result = new YamlSerializer().ReadFrom<SampleObject>(file);
		//testOutputHelper.WriteLine(result.Components.Count.ToString());
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

	// ReSharper disable all
#pragma warning disable CS0169 // Field is never used
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

	/// <summary>
	/// Demonstrates which access modifiers are supported by the serializers.
	/// </summary>
	/// <remarks>
	/// By default, only public/private properties and public fields are serializes.
	/// Private fields and readonly properties can be serialized by using the <see cref="SerializeFieldAttribute"/>.
	/// </remarks>
	private class AccessModifiersObject
	{
		#region Serialized

		public float PublicProperty { get; set; }
		private float PrivateProperty { get; set; }

		[field: SerializeField]
		public float PublicReadonlyPropertyWithSerialize { get; }

		[field: SerializeField]
		private float PrivateReadonlyPropertyWithSerialize { get; }

		[SerializeField]
		public float PublicFieldWithAttribute;

		[SerializeField]
		private float PrivateFieldWithAttribute;

		#endregion


		#region NotSerialized

		public float PublicField;

		private float PrivateField;

		public float PublicReadonlyProperty { get; }

		private float PrivateReadonlyProperty { get; }

		public readonly float PublicReadonlyField;
		private readonly float privateReadonlyField;

		[DontSerialize]
		public float PublicPropertyWithDontSerialize { get; set; }

		[DontSerialize]
		private float PrivatePropertyWithDontSerialize { get; set; }

		#endregion

		public void AssertDeserializedCorrectly(AccessModifiersObject source)
		{
			PublicProperty.Should().Be(source.PublicProperty);
			PrivateProperty.Should().Be(source.PrivateProperty);
			PublicReadonlyPropertyWithSerialize.Should().Be(source.PublicReadonlyPropertyWithSerialize);
			PrivateReadonlyPropertyWithSerialize.Should().Be(source.PrivateReadonlyPropertyWithSerialize);
			PublicFieldWithAttribute.Should().Be(source.PublicFieldWithAttribute);
			PrivateFieldWithAttribute.Should().Be(source.PrivateFieldWithAttribute);

			PublicField.Should().Be(0);
			privateReadonlyField.Should().Be(0);
			PublicReadonlyProperty.Should().Be(0);
			PrivateReadonlyProperty.Should().Be(0);
			PublicReadonlyField.Should().Be(0);
			PublicPropertyWithDontSerialize.Should().Be(0);
			PrivatePropertyWithDontSerialize.Should().Be(0);
		}

		public AccessModifiersObject()
		{
		}

		public AccessModifiersObject(float value)
		{
			foreach (FieldInfo field in GetType()
				         .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				field.SetValue(this, value);
			}

			foreach (PropertyInfo property in GetType()
				         .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				if (property.CanWrite)
					property.SetValue(this, value);
			}
		}
	}

	/// <summary>
	/// Demonstrates type scenarios that the serializers support.
	/// </summary>
	private class SampleObject
	{
		public string StringProp { get; set; } = "MyString";
		public float FloatProp { get; set; } = 1.2f;
		public int IntProp { get; set; } = 42;
		public bool BoolProp { get; set; } = true;
		public Vector2 Vector2Prop { get; set; } = new(1, 2);
		public List<Vector2> ListOfVector2s { get; set; } = new() { new(1, 2), new(3, 4) };
		public Dictionary<string, int> Dict { get; set; } = new() { { "a", 1 }, { "b", 2 } };
		public Dictionary<Coord, int> Tiles { get; set; } = new() { { new(1, 2), 3 }, { new(4, 5), 6 } };
		public List<Component> Components { get; set; } = new() { new ChildComponentANew(), new ChildComponentB_New() };
		public Rect RectProp { get; set; } = new(1, 2, 3, 4);

		[SerializeDerivedTypes]
		public abstract class Component
		{
			public string NameInBaseComponent { get; set; } = "BaseName";
		}

		[RenamedFrom("CatLands.Tests.SerializerTests+ChildComponentA")]
		public class ChildComponentANew : Component
		{
			public float UniquePropInChildA { get; set; } = 1.2f;
		}

		[RenamedFrom("CatLands.Tests.SerializerTests+ChildComponentB")]
		[RenamedFrom("CatLands.Tests.SerializerTests+ChildComponentOld")]
		public class ChildComponentB_New : Component
		{
			public int UniquePropInChildB { get; set; } = 42;
		}
	}
}