namespace CatLands.Shared;

using ImGuiNET;

public static class EnumCombo<T> where T : Enum
{
	// ReSharper disable once StaticMemberInGenericType
	private static readonly string[] names;

	static EnumCombo()
	{
		names = Enum.GetNames(typeof(T));
	}

	public static T Draw(string label, T value)
	{
		int currentIndex = Convert.ToInt32(value);
		if (ImGui.Combo(label, ref currentIndex, names, names.Length))
		{
			value = (T)Enum.ToObject(value.GetType(), currentIndex);
		}
		return value;
	}
}