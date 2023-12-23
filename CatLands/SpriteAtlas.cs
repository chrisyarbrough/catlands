namespace CatLands;

using System.Numerics;
using AutoMapper;
using Raylib_cs;

public class SpriteAtlasDto
{
	public List<Rectangle>? SpriteRects { get; set; }
	public List<AnimationDto>? Animations { get; set; }

	public void Create(SpriteAtlas instance)
	{
		var config = new MapperConfiguration(cfg =>
		{
			cfg.CreateMap<SpriteAtlasDto, SpriteAtlas>();
			cfg.CreateMap<AnimationDto, Animation>();
			cfg.CreateMap<AnimationDto.FrameDto, Animation.Frame>();
		});

		IMapper mapper = config.CreateMapper();
		mapper.Map(this, instance);
	}
	
	public static SpriteAtlasDto CreateFrom(SpriteAtlas spriteAtlas)
	{
		var config = new MapperConfiguration(cfg =>
		{
			cfg.CreateMap<SpriteAtlas, SpriteAtlasDto>();
			cfg.CreateMap<Animation, AnimationDto>();
			cfg.CreateMap<Animation.Frame, AnimationDto.FrameDto>();
		});

		IMapper mapper = config.CreateMapper();
		return mapper.Map<SpriteAtlasDto>(spriteAtlas);
	}
}

public class AnimationDto
{
	public string Name { get; set; } = string.Empty;
	public List<FrameDto>? Frames { get; set; }

	public class FrameDto
	{
		public int TileId { get; set; }
		public float Duration { get; set; }
	}
}

public class SpriteAtlas : IMementoOwner
{
	public readonly string TextureFilePath;
	public int Width => texture.Width;
	public int Height => texture.Height;
	public Texture2D Texture => texture;

	public List<Rect> SpriteRects
	{
		get => spriteRects;
		set => spriteRects = value;
	}

	public Vector2 TextureSize => new(texture.Width, texture.Height);
	public IList<Animation> Animations => animations;

	private Texture2D texture;

	private List<Rect> spriteRects = new();

	private List<Animation> animations = new();

	public string CreateMemento()
	{
		return YamlSerializer.Serialize(SpriteAtlasDto.CreateFrom(this));
	}

	public void RestoreState(string json)
	{
		this.spriteRects = new List<Rect>();
		this.animations = new List<Animation>();

		var v = YamlSerializer.Deserialize<SpriteAtlasDto>(json);
		v.Create(this);
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
				var memento = SerializedAsset.Load<SpriteAtlasDto>(spritesSavePath);
				memento.Create(this);
				return;
			}
		}
	}

	public void GenerateSpriteRects(Action<Texture2D, List<Rect>> slicer)
	{
		spriteRects.Clear();
		slicer.Invoke(texture, spriteRects);
	}

	public int Add(Animation animation)
	{
		int id = animations.Count;
		animations.Add(animation);
		return id;
	}

	public int Add(Rectangle spriteRect)
	{
		int id = spriteRects.Count;
		spriteRects.Add(spriteRect);
		return id;
	}

	public void RemoveTile(int tileId)
	{
		spriteRects.RemoveAt(tileId);

		foreach (Animation animation in animations)
			animation.Frames.RemoveAll(frame => frame.TileId == tileId);
	}

	public bool GetRenderInfo(int tileId, out Vector2 size, out Vector2 uv0, out Vector2 uv1)
	{
		if (tileId < 0 || tileId >= spriteRects.Count)
		{
			size = Vector2.Zero;
			uv0 = Vector2.Zero;
			uv1 = Vector2.Zero;
			return false;
		}

		Rectangle rect = spriteRects[tileId];
		size = new Vector2(rect.Width, rect.Height);
		uv0 = new Vector2(rect.X / texture.Width, rect.Y / texture.Height);
		uv1 = new Vector2(rect.xMax() / texture.Width, rect.yMax() / texture.Height);
		return true;
	}
}