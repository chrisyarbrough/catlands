namespace CatLands;

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