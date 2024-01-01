namespace CatLands;

using AutoMapper;

public class SpriteAtlasDto
{
	public List<Rect>? SpriteRects { get; set; }
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