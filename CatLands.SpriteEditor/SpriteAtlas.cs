namespace CatLands.SpriteEditor;

using Newtonsoft.Json;
using Raylib_cs;

public class SpriteAtlas
{
	public readonly string TextureFilePath;
	public int Width => texture.Width;
	public int Height => texture.Height;
	public Texture2D Texture => texture;
	public List<Rectangle> SpriteRects => spriteRects;

	private readonly Texture2D texture;
	private readonly List<Rectangle> spriteRects;

	public void Save()
	{
		string json = JsonConvert.SerializeObject(spriteRects, Formatting.Indented);
		string spritesSavePath = Path.ChangeExtension(TextureFilePath, ".json");
		File.WriteAllText(spritesSavePath, json);
	}
	
	public static SpriteAtlas Load(string filePath)
	{
		Console.WriteLine("Loading sprite atlas: " + filePath);
		return new SpriteAtlas(filePath);
	}

	private SpriteAtlas(string textureFilePath)
	{
		this.TextureFilePath = textureFilePath;
		this.texture = Raylib.LoadTexture(textureFilePath);

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