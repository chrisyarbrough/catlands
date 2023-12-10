namespace CatLands.SpriteEditor;

using System.Numerics;
using ImGuiNET;
using Raylib_cs;

public class CheckerBackground
{
	private Texture2D checkerTexture1;
	private Texture2D checkerTexture2;
	private int checkerAlpha = 128;
	private int checkerBaseValue = 128;

	public CheckerBackground()
	{
		SettingsWindow.Add("Background", DrawSettings);
	}

	private void DrawSettings()
	{
		ImGui.SliderInt("Alpha", ref checkerAlpha, 0, 255);
		if (ImGui.SliderInt("Value", ref checkerBaseValue, 0, 255))
		{
			checkerTexture1 = Refresh(checkerTexture1.Size(), new Vector2(1, 1));
			checkerTexture2 = Refresh(checkerTexture1.Size(), new Vector2(16, 16));
		}
	}

	public void DrawScene(Vector2 position)
	{
		Raylib.DrawTextureV(checkerTexture1, position, new Color(255, 255, 255, checkerAlpha));
		Raylib.DrawTextureV(checkerTexture2, position, new Color(255, 255, 255, (int)(checkerAlpha * 0.75f)));
	}

	public void Refresh(Vector2 textureSize)
	{
		checkerTexture1 = Refresh(textureSize, new Vector2(1, 1));
		checkerTexture2 = Refresh(textureSize, new Vector2(16, 16));
	}

	public Texture2D Refresh(Vector2 textureSize, Vector2 checkerSize)
	{
		const int contrast = 40;
		int highValue = Math.Clamp(checkerBaseValue + contrast, 0, 255);
		int lowValue = Math.Clamp(checkerBaseValue - contrast, 0, 255);
		Image checkerImage = Raylib.GenImageChecked((int)textureSize.X, (int)textureSize.Y, (int)checkerSize.X, (int)checkerSize.Y,
			new Color(lowValue, lowValue, lowValue, 255),
			new Color(highValue, highValue, highValue, 255));
		Texture2D texture = Raylib.LoadTextureFromImage(checkerImage);
		Raylib.UnloadImage(checkerImage);
		return texture;
	}
}