namespace CatLands;

using System.Numerics;
using Raylib_cs;

public class SpriteAtlas : IMementoOwner
{
	public readonly string TextureFilePath;
	public int Width => texture.Width;
	public int Height => texture.Height;
	public Texture2D Texture => texture;

	public int SpriteCount => sprites.Count;
	
	[DontSerialize]
	public Rect this[int id]
	{
		get => sprites[id];
		set => sprites[id] = value;
	}
	
	public IEnumerable<(int, Rect)> Sprites => sprites.Select(kvp => (kvp.Key, kvp.Value));

	public Rect GetSprite(int id) => sprites[id];
	
	public bool HasSprite(int id) => sprites.ContainsKey(id);
	public void SetSprite(int id, Rect rect) => sprites[id] = rect;

	public Vector2 TextureSize => new(texture.Width, texture.Height);
	public IList<Animation> Animations => animations;

	private Texture2D texture;

	[SerializeField]
	private Dictionary<int, Rect> sprites = new();

	[SerializeField]
	private List<Animation> animations = new();

	public SpriteAtlas()
	{
	}

	public string CreateMemento()
	{
		return YamlSerializer.Serialize(this);
	}

	public void RestoreState(string json)
	{
		var v = YamlSerializer.Deserialize<SpriteAtlas>(json);
		this.sprites = v.sprites ?? new();
		this.animations = v.animations ?? new();
	}

	public void Save()
	{
		string json = CreateMemento();
		string spritesSavePath = Path.ChangeExtension(TextureFilePath, ".yaml");
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

		foreach (string extension in SerializedAsset.SupportedFileExtensions)
		{
			string spritesSavePath = Path.ChangeExtension(textureFilePath, extension);
			if (File.Exists(spritesSavePath))
			{
				string yaml = File.ReadAllText(spritesSavePath);
				RestoreState(yaml);
				return;
			}
		}
	}

	public void GenerateSpriteRects(Func<Texture2D, IEnumerable<Rect>> slicer)
	{
		sprites.Clear();
		int nextSpriteId = 1;
		foreach (Rect rect in slicer.Invoke(texture))
		{
			sprites.Add(nextSpriteId, rect);
			nextSpriteId++;
		}
	}

	public int Add(Animation animation)
	{
		int id = animations.Count;
		animations.Add(animation);
		return id;
	}

	public int Add(Rectangle spriteRect)
	{
		int id = sprites.Count > 0 ? sprites.Keys.Max() + 1 : 1;
		sprites.Add(id, spriteRect);
		return id;
	}

	public void RemoveTile(int tileId)
	{
		sprites.Remove(tileId);

		foreach (Animation animation in animations)
			animation.Frames.RemoveAll(frame => frame.TileId == tileId);
	}

	public bool GetRenderInfo(int tileId, out Vector2 size, out Vector2 uv0, out Vector2 uv1)
	{
		if (tileId < 0 || tileId >= sprites.Count)
		{
			size = Vector2.Zero;
			uv0 = Vector2.Zero;
			uv1 = Vector2.Zero;
			return false;
		}

		Rectangle rect = sprites[tileId];
		size = new Vector2(rect.Width, rect.Height);
		uv0 = new Vector2(rect.X / texture.Width, rect.Y / texture.Height);
		uv1 = new Vector2(rect.xMax() / texture.Width, rect.yMax() / texture.Height);
		return true;
	}
}