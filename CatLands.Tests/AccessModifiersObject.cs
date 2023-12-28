namespace CatLands.Tests;

using System.Reflection;

// ReSharper disable all
#pragma warning disable

/// <summary>
/// Demonstrates all of member configurations that the serializer supports.
/// </summary>
/// <remarks>
/// By default, only public/private properties are serialized.
/// Private fields and readonly properties can be serialized by using the <see cref="SerializeFieldAttribute"/>.
/// </remarks>
internal class AccessModifiersObject
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

	// Requires by serializer.
	public AccessModifiersObject()
	{
	}

	// Called by tests to create an instance which has values applied to all members, which is then serialized.
	// After deserialization, if any of the members are still 0, we know that deserialized failed.
	public AccessModifiersObject(float value)
	{
		BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		foreach (FieldInfo field in GetType().GetFields(flags))
		{
			field.SetValue(this, value);
		}

		foreach (PropertyInfo property in GetType().GetProperties(flags))
		{
			if (property.CanWrite)
				property.SetValue(this, value);
		}
	}

	public void AssertDeserializedCorrectly(AccessModifiersObject source)
	{
		// The source is the original object which has every member set to a value.
		// The following values should be correctly restored from the serialized data.
		PublicProperty.Should().Be(source.PublicProperty);
		PrivateProperty.Should().Be(source.PrivateProperty);
		PublicReadonlyPropertyWithSerialize.Should().Be(source.PublicReadonlyPropertyWithSerialize);
		PrivateReadonlyPropertyWithSerialize.Should().Be(source.PrivateReadonlyPropertyWithSerialize);
		PublicFieldWithAttribute.Should().Be(source.PublicFieldWithAttribute);
		PrivateFieldWithAttribute.Should().Be(source.PrivateFieldWithAttribute);

		// These members should remain untouched by the serializer (uninitialized).
		PublicField.Should().Be(0);
		privateReadonlyField.Should().Be(0);
		PublicReadonlyProperty.Should().Be(0);
		PrivateReadonlyProperty.Should().Be(0);
		PublicReadonlyField.Should().Be(0);
		PublicPropertyWithDontSerialize.Should().Be(0);
		PrivatePropertyWithDontSerialize.Should().Be(0);
	}
}

#pragma warning restore