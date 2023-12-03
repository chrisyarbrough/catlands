using System.Numerics;
using CatLands;
using CatLands.SpriteEditor;
using ImGuiNET;
using Raylib_cs;
using RectpackSharp;

public class ColorDilation
{
	private Shader shader;
	private bool initialized;

	private bool useShader;

	public void Draw(SpriteAtlas spriteAtlas)
	{
		DrawPreview(spriteAtlas);

		PackingRectangle[] rectangles = new PackingRectangle[spriteAtlas.SpriteRects.Count];
		for (int i = 0; i < spriteAtlas.SpriteRects.Count; i++)
		{
			Rectangle rect = spriteAtlas.SpriteRects[i];
			rectangles[i] = new PackingRectangle((uint)rect.X, (uint)rect.Y, (uint)rect.Width, (uint)rect.Height, i);
		}
		
		// RectanglePacker.Pack(rectangles, out PackingRectangle bounds, PackingHints.MostlySquared);

		foreach (var r in rectangles)
		{
			Raylib.DrawRectangleLines((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height, Color.RED);
		}
	
		
		return;
		Texture2D originalTileset = spriteAtlas.Texture;
		int borderSize = 2;
		int tilesPerRow = originalTileset.Width / 16; // Assuming the width is a multiple of 16
		int tilesPerColumn = originalTileset.Height / 16; // Assuming the height is a multiple of 16

		int newWidth = originalTileset.Width + tilesPerRow * borderSize;
		int newHeight = originalTileset.Height + tilesPerColumn * borderSize;

		Raylib.SetTextureFilter(originalTileset, TextureFilter.TEXTURE_FILTER_POINT);

		RenderTexture2D target = Raylib.LoadRenderTexture(newWidth, newHeight);
		Raylib.SetTextureFilter(target.Texture, TextureFilter.TEXTURE_FILTER_POINT);

		Raylib.BeginTextureMode(target);
		Raylib.ClearBackground(Color.BLUE);

		// spriteRects.Clear();

		for (int y = 0; y < tilesPerColumn; ++y)
		{
			for (int x = 0; x < tilesPerRow; ++x)
			{
				// Source rectangle in the original tileset
				Rectangle sourceRec = new Rectangle(x * 16, y * 16, 16, -16);


				// Destination rectangle in the new texture
				Rectangle destRec =
					new Rectangle(x * (16 + borderSize), newHeight - (y + 1) * (16 + borderSize), 16, 16);

				Raylib.DrawTexturePro(originalTileset, sourceRec, destRec, Vector2.Zero, 0f, Color.WHITE);


				float newX = x * (16 + borderSize);
				float newY = y * (16 + borderSize) + 2;

				// Create a rectangle for the sprite
				Rectangle spriteRect = new Rectangle(newX, newY, 16, 16);

				// Add the rectangle to the list
				// spriteRects.Add(spriteRect);
			}
		}

		Raylib.EndTextureMode();
		Raylib.SetTextureWrap(target.Texture, TextureWrap.TEXTURE_WRAP_CLAMP);

		// Raylib.DrawTexture(target.Texture);
		// AllTextures.Add(target.Texture);
		//
		// this.texture = target.Texture;
	}

	private void DrawPreview(SpriteAtlas spriteAtlas)
	{
		if (!initialized)
		{
			shader = Raylib.LoadShader(null, "ColorDilate.glsl");
			initialized = true;
		}

		if (Raylib.IsKeyPressed(KeyboardKey.KEY_S))
			useShader = !useShader;

		ImGui.TextUnformatted("Use shader: " + useShader);

		if (useShader)
			Raylib.BeginShaderMode(shader);

		Raylib.DrawTextureEx(spriteAtlas.Texture, Vector2.Zero, 0f, scale: 1f, Color.WHITE);

		if (useShader)
			Raylib.EndShaderMode();
	}
}