namespace CatLands;

public class App
{
	public string Title { get; set; } = string.Empty;

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