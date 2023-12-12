namespace CatLands.SpriteEditor;

public class Animation
{
	public string Name
	{
		get => name;
		set => name = value;
	}

	public List<Frame> Frames
	{
		get => frames;
		set => frames = value;
	}

	public TimeSpan Duration
	{
		get
		{
			float totalDuration = frames.Sum(frame => frame.Duration);
			return TimeSpan.FromSeconds(totalDuration);
		}
	}

	public int FrameCount => frames.Count;

	public bool Loop = true;

	public Frame FrameAt(int index) => frames[index];

	private string name = string.Empty;
	private List<Frame> frames = new();

	public class Frame
	{
		public int TileId;

		public float Duration
		{
			get => duration;
			set => duration = Math.Max(0, value);
		}

		private float duration;

		public Frame(int tileId, float duration)
		{
			this.TileId = tileId;
			this.Duration = duration;
		}
	}
}