namespace CatLands;

using System.Numerics;
using Raylib_cs;

internal class Program
{
	public static void Main()
	{
		Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE | ConfigFlags.FLAG_VSYNC_HINT);
		Raylib.InitWindow(width: 1280, height: 800, title: "CatLands");
		Raylib.SetTargetFPS(60);

		var systems = new List<ISystem>
		{
			new MapDisplay()
		};

		Camera2D camera = new Camera2D(Vector2.Zero, Vector2.Zero, 0f, 1f);

		while (!Raylib.WindowShouldClose())
		{
			float deltaTime = Raylib.GetFrameTime();
			systems.ForEach(system => system.Update(deltaTime));

			Raylib.BeginDrawing();
			Raylib.BeginMode2D(camera);
			systems.ForEach(system => system.Draw(camera));
			Raylib.EndMode2D();
			Raylib.EndDrawing();
		}

		Raylib.CloseWindow();
	}
}