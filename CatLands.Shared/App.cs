namespace CatLands;

public class App
{
	public string BaseTitle { get; init; } = string.Empty;
	public string SubTitle { get; set; } = string.Empty;

	public string CombinedTitle
	{
		get
		{
			string t = BaseTitle;

			if (SubTitle.Length > 0)
				t += " - " + SubTitle;

			return t;
		}
	}

	protected string[] CommandLineArgs => Environment.GetCommandLineArgs();

	public virtual void Initialize()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void Shutdown()
	{
	}
}