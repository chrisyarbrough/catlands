namespace CatLands;

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
	public IList<Animation> Animations => animations;

	private Texture2D texture;
	private List<Rectangle> spriteRects = new();
	private List<Animation> animations = new();

	public string GetMemento()
	{
		return JsonConvert.SerializeObject(new { spriteRects, animations }, Formatting.Indented);
	}

	public void SetMemento(string json)
	{
		var v = new { spriteRects, animations };
		v = JsonConvert.DeserializeAnonymousType(json, v)!;
		this.spriteRects = v.spriteRects;
		this.animations = v.animations;
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

	public void Add(Animation animation)
	{
		animations.Add(animation);
	}

	public int Add(Rectangle spriteRect)
	{
		int id = spriteRects.Count;
		spriteRects.Add(spriteRect);
		return id;
	}

	public void GetRenderInfo(int tileId, out Vector2 size, out Vector2 uv0, out Vector2 uv1)
	{
		Rectangle rect = spriteRects[tileId];
		size = new Vector2(rect.Width, rect.Height);
		uv0 = new Vector2(rect.X / texture.Width, rect.Y / texture.Height);
		uv1 = new Vector2(rect.xMax() / texture.Width, rect.yMax() / texture.Height);
	}
}