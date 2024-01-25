using Raylib_cs;

public class ShaderAsset
{
	private struct DrawingScopeInternal : IDisposable
	{
		public DrawingScopeInternal(ShaderAsset shaderAsset)
		{
			shaderAsset.Begin();
		}

		public void Dispose()
		{
			Raylib.EndShaderMode();
		}
	}

	public IDisposable DrawingBlock() => new DrawingScopeInternal(this);

	public Shader Shader => shader;

	private readonly string vsFileName;
	private readonly string fsFileName;
	private readonly Action<Shader> getLocations;
	private readonly FileSystemWatcher watcher;

	private Shader shader;
	private bool queueReload;

	public ShaderAsset(string vsFileName, string fsFileName, Action<Shader> getLocations = null)
	{
		this.vsFileName = vsFileName;
		this.fsFileName = fsFileName;
		this.getLocations = getLocations;

		shader = Raylib.LoadShader(vsFileName, fsFileName);
		getLocations?.Invoke(shader);

		watcher = new FileSystemWatcher
		{
			Path = Path.GetDirectoryName(fsFileName) ?? throw new ArgumentException("Must be a file path."),
			Filter = "*",
			NotifyFilter = NotifyFilters.LastWrite,
			EnableRaisingEvents = true,
		};
		watcher.Changed += OnChanged;
	}

	public void Begin()
	{
		Raylib.BeginShaderMode(Shader);

		if (queueReload)
		{
			Raylib.UnloadShader(shader);
			shader = Raylib.LoadShader(vsFileName, fsFileName);
			getLocations?.Invoke(shader);
			queueReload = false;
		}
	}

	public void End()
	{
		Raylib.EndShaderMode();
	}

	~ShaderAsset()
	{
		if (watcher != null)
		{
			watcher.Changed -= OnChanged;
			watcher.Dispose();
		}

		Raylib.UnloadShader(shader);
	}

	private void OnChanged(object sender, FileSystemEventArgs e)
	{
		if (e.FullPath == vsFileName || e.FullPath == fsFileName)
		{
			Console.WriteLine($"Reloading shader '{e.FullPath}'");
			queueReload = true;
		}
	}

	public static implicit operator Shader(ShaderAsset asset) => asset.Shader;
}