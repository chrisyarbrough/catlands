namespace CatLands.SpriteEditor;

using System.Numerics;
using Raylib_cs;

public static class LabelUtility
{
	private static Font font;

	private const float spacing = 0f;

	public static Vector2 Measure(string text, float fontSize)
	{
		return Raylib.MeasureTextEx(font, text, fontSize, spacing);
	}

	public static void Draw(string text, Vector2 pos, Vector2 shadowOffset, float fontSize)
	{
		InitializeFont();
		Raylib.DrawTextEx(font, text, pos + shadowOffset, fontSize, spacing, Color.BLACK);
		Raylib.DrawTextEx(font, text, pos, fontSize, spacing, Color.WHITE);
	}

	private static void InitializeFont()
	{
		if (font.Texture.Id == 0)
		{
			font = Raylib.LoadFontEx("Fonts/liberation-mono/LiberationMono-Regular.ttf", 128, null, 0);
			Raylib.SetTextureFilter(font.Texture, TextureFilter.TEXTURE_FILTER_BILINEAR);
		}
	}
}