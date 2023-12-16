using System.Numerics;
using ImGuiNET;

public static class U32Color
{
	public static readonly uint Red;
	public static readonly uint Green;
	public static readonly uint Blue;
	public static readonly uint White;
	public static readonly uint Yellow;
	public static readonly uint Orange;

	static U32Color()
	{
		Red = FromRGB(1f, 0f, 0f);
		Green = FromRGB(0f, 1f, 0f);
		Blue = FromRGB(0f, 0f, 1f);
		White = Red | Green | Blue;
		Yellow = FromRGB(253 / 255f, 249 / 255f, 0f);
		Orange = FromRGB(1f, 161 / 255f, 0f);
	}

	/// <summary>
	/// Converts color components [0..1] to a U32 representation.
	/// </summary>
	public static uint FromRGB(float r, float g, float b) => ImGui.ColorConvertFloat4ToU32(new Vector4(r, g, b, 1f));

	public static uint FromValue(float value) => FromRGB(value, value, value);
}