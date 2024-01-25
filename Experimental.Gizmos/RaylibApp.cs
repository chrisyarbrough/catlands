namespace Experimental.Gizmos;

using Raylib_cs;

public abstract class RaylibApp<T> where T : EditModelBase
{
	protected string Title { get; set; } = string.Empty;

	protected string SubTitle
	{
		get => subTitle;
		set
		{
			subTitle = value;
			Raylib.SetWindowTitle(Title + SubTitle);
		}
	}

	protected T EditModel;

	private Model model;
	private string subTitle = string.Empty;

	protected void Initialize()
	{
		Title = GetType().Assembly.GetName().Name;
		Raylib.SetTraceLogLevel(TraceLogLevel.LOG_ERROR);
		Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
		Raylib.InitWindow(width: 1280, height: 800, Title + SubTitle);
		Raylib.SetTargetFPS(240);

		model = Model.Load();
		EditModel = (T)Activator.CreateInstance(typeof(T), model);
		EditModel!.Changed += OnModelChanged;
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
			Rect rect = Rect.Handle(Raylib.GetMousePosition(), size: 50);
			EditModel.AddRect(rect);
		}

		if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SUPER) && Raylib.IsKeyPressed(KeyboardKey.KEY_Z))
		{
			EditModel.Undo();
		}

		if (Raylib.IsKeyPressed(KeyboardKey.KEY_BACKSPACE))
		{
			EditModel.DeleteSelected();
		}
	}

	public void OnModelChanged(bool isDirty)
	{
		SubTitle = isDirty ? "*" : "";
	}

	protected abstract void Update();

	protected void Shutdown()
	{
		EditModel.Save();
		Raylib.CloseWindow();
	}
}