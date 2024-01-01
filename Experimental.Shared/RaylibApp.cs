using Raylib_cs;

public abstract class RaylibApp<T> where T : EditModelBase
{
	protected Model model;
	protected T editModel;

	protected virtual string Title { get; set; } = string.Empty;

	protected string SubTitle
	{
		get => subTitle;
		set
		{
			subTitle = value;
			Raylib.SetWindowTitle(Title + SubTitle);
		}
	}

	private string subTitle = string.Empty;

	protected virtual void Initialize()
	{
		Title = GetType().Assembly.GetName().Name;
		Raylib.SetTraceLogLevel(TraceLogLevel.LOG_WARNING);
		Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE | ConfigFlags.FLAG_VSYNC_HINT);
		Raylib.InitWindow(width: 1280, height: 800, Title + SubTitle);
		Raylib.SetTargetFPS(120);

		model = Model.Load();
		editModel = (T)Activator.CreateInstance(typeof(T), model);
		editModel.Changed += OnModelChanged;
	}

	public void Run()
	{
		Initialize();

		while (!Raylib.WindowShouldClose())
		{
			Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.DARKGRAY);
			HandleKeyboard();
			Update();
			Raylib.EndDrawing();
		}

		Shutdown();
	}

	private void HandleKeyboard()
	{
		if (Raylib.IsKeyPressed(KeyboardKey.KEY_A))
		{
			editModel.AddItem(Raylib.GetMousePosition(), 50f);
		}

		if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SUPER) && Raylib.IsKeyPressed(KeyboardKey.KEY_Z))
		{
			editModel.Undo();
		}

		if (Raylib.IsKeyPressed(KeyboardKey.KEY_BACKSPACE))
		{
			editModel.DeleteSelected();
		}
	}

	public void OnModelChanged(bool isDirty)
	{
		SubTitle = isDirty ? "*" : "";
	}

	protected abstract void Update();

	protected virtual void Shutdown()
	{
		editModel.Save();
		Raylib.CloseWindow();
	}
}