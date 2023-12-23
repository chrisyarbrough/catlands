namespace CatLands.Tests;

using System.Numerics;

// ReSharper disable all
#pragma warning disable

/// <summary>
/// Demonstrates type scenarios that the serializers support.
/// </summary>
internal class SampleObject
{
	[RenamedFrom("StringProp")]
	[RenamedFrom("StringPropOld")]
	public string StringPropNew { get; set; }

	public float FloatProp { get; set; }
	public int IntProp { get; set; }
	public bool BoolProp { get; set; }
	public Vector2 Vector2Prop { get; set; }
	public List<Vector2> ListOfVector2s { get; set; }
	public Dictionary<string, int> Dict { get; set; }
	public Dictionary<Coord, int> Tiles { get; set; }
	public List<Component> Components { get; set; }
	public Rect RectProp { get; set; }

	[SerializeTypeHierarchy]
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