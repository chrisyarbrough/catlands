namespace CatLands.SpriteEditor;

using System.Numerics;
using Newtonsoft.Json;
using Raylib_cs;

public class SpriteAtlas
{
	public readonly string TextureFilePath;
	public int Width => texture.Width;
	public int Height => texture.Height;
	public Texture2D Texture => texture;

	public List<Rectangle> SpriteRects
	{
		get => spriteRects;
		set => spriteRects = value;
	}

	public Vector2 TextureSize => new(texture.Width, texture.Height);

	private Texture2D texture;
	private List<Rectangle> spriteRects = new();

	public string GetMemento()
	{
		return JsonConvert.SerializeObject(spriteRects, Formatting.Indented);
	}

	public void SetMemento(string json)
	{
		spriteRects = JsonConvert.DeserializeObject<List<Rectangle>>(json)!;
	}

	public void Save()
	{
		string json = GetMemento();
		string spritesSavePath = Path.ChangeExtension(TextureFilePath, ".json");
		File.WriteAllText(spritesSavePath, json);
	}

	public static SpriteAtlas Load(string textureFilePath)
	{
		Console.WriteLine("Loading sprite atlas: " + textureFilePath);
		
		if (textureFilePath.EndsWith(".json"))
			throw new ArgumentException("Sprite atlas should be loaded by passing the texture file path.");
		
		return new SpriteAtlas(textureFilePath);
	}

	private SpriteAtlas(string textureFilePath)
	{
		this.TextureFilePath = textureFilePath;
		this.texture = Raylib.LoadTexture(textureFilePath);
		Raylib.SetTextureWrap(texture, TextureWrap.TEXTURE_WRAP_CLAMP);
		Raylib.SetTextureFilter(texture, TextureFilter.TEXTURE_FILTER_POINT);

		string spriteFilePath = Path.ChangeExtension(textureFilePath, ".json");
		if (File.Exists(spriteFilePath))
		{
			string json = File.ReadAllText(spriteFilePath);
			SetMemento(json);
		}
		else
		{
			this.spriteRects = new List<Rectangle>();
		}
	}

	public void GenerateSpriteRects(Action<Texture2D, List<Rectangle>> slicer)
	{
		spriteRects.Clear();
		slicer.Invoke(texture, spriteRects);
	}
}