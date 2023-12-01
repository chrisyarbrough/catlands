namespace CatLands.SpriteEditor;

using System.Numerics;
using Newtonsoft.Json;
using Raylib_cs;

public class SpriteAtlas
{
	public static List<Texture2D> AllTextures = new();
	
	public readonly string TextureFilePath;
	public int Width => texture.Width;
	public int Height => texture.Height;
	public Texture2D Texture => texture;
	public List<Rectangle> SpriteRects => spriteRects;

	private Texture2D texture;
	private readonly List<Rectangle> spriteRects;

	public void Save()
	{
		string json = JsonConvert.SerializeObject(spriteRects, Formatting.Indented);
		string spritesSavePath = Path.ChangeExtension(TextureFilePath, ".json");
		File.WriteAllText(spritesSavePath, json);
	}

	public static SpriteAtlas Load(string textureFilePath)
	{
		Console.WriteLine("Loading sprite atlas: " + textureFilePath);
		return new SpriteAtlas(textureFilePath);
	}

	private SpriteAtlas(string textureFilePath)
	{
		if (textureFilePath.EndsWith(".json"))
			throw new ArgumentException("Sprite atlas should be loaded by passing the texture file path.");

		this.TextureFilePath = textureFilePath;
		this.texture = Raylib.LoadTexture(textureFilePath);
		Raylib.GenTextureMipmaps(ref texture);
		Raylib.SetTextureWrap(texture, TextureWrap.TEXTURE_WRAP_CLAMP);
		Raylib.SetTextureFilter(texture, TextureFilter.TEXTURE_FILTER_BILINEAR);
		// AllTextures.Add(texture);

		

		string spriteFilePath = Path.ChangeExtension(textureFilePath, ".json");
		if (File.Exists(spriteFilePath))
		{
			string json = File.ReadAllText(spriteFilePath);
			this.spriteRects = JsonConvert.DeserializeObject<List<Rectangle>>(json)!;
		}
		else
		{
			this.spriteRects = new List<Rectangle>();
		}
		
		// DoMagic(texture);
	}

	private void DoMagic(Texture2D originalTileset)
	{
		int borderSize = 2;
		int tilesPerRow = originalTileset.Width / 16; // Assuming the width is a multiple of 16
		int tilesPerColumn = originalTileset.Height / 16; // Assuming the height is a multiple of 16

		int newWidth = originalTileset.Width + tilesPerRow * borderSize;
		int newHeight = originalTileset.Height + tilesPerColumn * borderSize;
		
		Raylib.SetTextureFilter(originalTileset, TextureFilter.TEXTURE_FILTER_POINT);

		RenderTexture2D target = Raylib.LoadRenderTexture(newWidth, newHeight);
		Raylib.SetTextureFilter(target.Texture, TextureFilter.TEXTURE_FILTER_POINT);
		
		Raylib.BeginTextureMode(target);
		Raylib.ClearBackground(Color.BLUE); // Clear to transparent or your desired background color
		
		spriteRects.Clear();

		Image image = Raylib.LoadImage(TextureFilePath);
		

		for (int y = 0; y < tilesPerColumn; ++y) {
			for (int x = 0; x < tilesPerRow; ++x) {
				// Source rectangle in the original tileset
				Rectangle sourceRec = new Rectangle(x * 16, y * 16, 16, -16);

				

				// Destination rectangle in the new texture
				Rectangle destRec = new Rectangle(x * (16 + borderSize), newHeight - (y + 1) * (16 + borderSize), 16, 16);
				
				

				// Draw tile to the new texture
				Rectangle r = destRec;
				r.X -= 1;
				r.Width += 2;
				r.Y -= 1;
				r.Height += 2;
				for (int i = 0; i < 16; i++)
				{
					for (int j = 0; j < 16; j++)
					{
						Color color = Raylib.GetImageColor(image, (int)sourceRec.X + i, (int)sourceRec.Y + j);
						
					}
				}
				Raylib.DrawTexturePro(originalTileset, sourceRec, destRec, Vector2.Zero, 0f, Color.WHITE);
				
				


				float newX = x * (16 + borderSize);
				float newY = y * (16 + borderSize) + 2;

				// Create a rectangle for the sprite
				Rectangle spriteRect = new Rectangle(newX, newY, 16, 16);
        
				// Add the rectangle to the list
				spriteRects.Add(spriteRect);
			}
		}

		Raylib.EndTextureMode();
		Raylib.SetTextureWrap(target.Texture, TextureWrap.TEXTURE_WRAP_CLAMP);
		AllTextures.Add(target.Texture);

		this.texture = target.Texture;
	}

	public void Slice(int tileSizeX, int tileSizeY)
	{
		spriteRects.Clear();

		int cols = texture.Width / tileSizeX;
		int rows = texture.Height / tileSizeY;

		for (int y = 0; y < rows; y++)
		{
			for (int x = 0; x < cols; x++)
			{
				var tile = new Rectangle(x * tileSizeX, y * tileSizeY, tileSizeX, tileSizeY);
				spriteRects.Add(tile);
			}
		}
	}
}