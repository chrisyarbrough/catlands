using Raylib_cs;

public static class TextureUtility
{
	public static Texture2D GenTextureColor(int width, int height, Color color)
	{
		Image image = Raylib.GenImageColor(width, height, color);
		return Raylib.LoadTextureFromImage(image);
	}
	
	public static Texture2D GenTextureChecker(int width, int height, int count = 10)
	{
		Image image = Raylib.GenImageChecked(width, height, count, count, Color.BLACK, Color.WHITE);
		return Raylib.LoadTextureFromImage(image);
	}
}