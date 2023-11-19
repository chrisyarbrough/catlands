namespace CatLands;

public static class Program
{
	static void Main()
	{
	}
}
//
// using System.Numerics;
// using Raylib_cs;
//
// public class Program
// {
// 	private const int screenWidth = 1000;
// 	private const int screenHeight = 480;
// 	private static Texture2D grassSprite;
// 	private static Texture2D playerSprite;
// 	private static Rectangle playerSrc;
// 	private static Rectangle playerDst;
// 	private const float playerSpeed = 3f;
// 	private static Camera2D camera;
// 	private static bool playerMoving;
// 	private static int playerDir;
// 	private static bool playerUp, playerDown, playerRight, playerLeft;
// 	private static int frameCount;
// 	private static int playerFrame;
// 	private static Rectangle tileSrc;
// 	private static Rectangle tileDest;
// 	private static List<int> tilemap = new();
// 	private static string[] sourceMap; // What type of tile by defining which file.
// 	private static int mapW, mapH;
//
// 	public static void Main()
// 	{
// 		
// 		return;
// 		Raylib.SetTraceLogLevel(TraceLogLevel.LOG_WARNING);
//
// 		Raylib.SetConfigFlags(ConfigFlags.FLAG_VSYNC_HINT);
// 		Raylib.InitWindow(screenWidth, screenHeight, "Cat Lands");
//
// 		// Prevent ESC from closing the window because we might want to use it for in-game ui.
// 		// Must be set after InitWindow.
// 		Raylib.SetExitKey(0);
//
// 		Raylib.SetTargetFPS(60);
//
// 		Console.WriteLine(Directory.GetCurrentDirectory());
// 		grassSprite = Raylib.LoadTexture("res/Tilesets/ground tiles/Old tiles/Grass.png");
// 		playerSprite = Raylib.LoadTexture("res/Characters/Basic Charakter Spritesheet.png");
// 		playerSrc = new Rectangle(0, 0, 48, 48);
// 		playerDst = new Rectangle(200, 200, 100, 100);
//
// 		tileDest = new Rectangle(0, 0, 16, 16);
// 		tileSrc = new Rectangle(0, 0, 16, 16);
//
// 		LoadMap("first.map");
//
// 		camera = new Camera2D(
// 			offset: new Vector2(screenWidth / 2f, screenHeight / 2f),
// 			target: new Vector2(),
// 			rotation: 0f,
// 			zoom: 1.5f);
//
// 		Raylib.InitAudioDevice();
// 		Music music = Raylib.LoadMusicStream("res/Audio/Aeronauts OST Exploration.mp3");
// 		// Raylib.PlayMusicStream(music);
//
// 		while (!Raylib.WindowShouldClose())
// 		{
// 			Raylib.UpdateMusicStream(music);
// 			HandleInput();
// 			Update();
// 			Render();
// 		}
//
// 		Raylib.UnloadMusicStream(music);
// 		Raylib.CloseAudioDevice();
// 		Raylib.UnloadTexture(playerSprite);
// 		Raylib.UnloadTexture(grassSprite);
// 	}
//
// 	private static void LoadMap(string mapFilePath)
// 	{
// 		string text = File.ReadAllText(mapFilePath);
//
// 		using (var reader = new StringReader(text))
// 		{
// 			string[] widthAndHeight = reader.ReadLine()!.Split(" ");
// 			mapW = int.Parse(widthAndHeight[0]);
// 			mapH = int.Parse(widthAndHeight[1]);
//
// 			string? row = null;
// 			do
// 			{
// 				row = reader.ReadLine();
//
// 				if (row == null)
// 					break;
//
// 				int[] rowValues = row.Split(" ").Select(x => int.Parse(x)).ToArray();
// 				tilemap.AddRange(rowValues);
// 			} while (row != null);
// 		}
// 	}
//
// 	private static void HandleInput()
// 	{
// 		if (Raylib.IsKeyDown(KeyboardKey.KEY_W) || Raylib.IsKeyDown(KeyboardKey.KEY_UP))
// 		{
// 			playerDir = 1;
// 			playerUp = true;
// 			playerMoving = true;
// 		}
//
// 		if (Raylib.IsKeyDown(KeyboardKey.KEY_A) || Raylib.IsKeyDown(KeyboardKey.KEY_LEFT))
// 		{
// 			playerDir = 2;
// 			playerLeft = true;
// 			playerMoving = true;
// 		}
//
// 		if (Raylib.IsKeyDown(KeyboardKey.KEY_S) || Raylib.IsKeyDown(KeyboardKey.KEY_DOWN))
// 		{
// 			playerDir = 0;
// 			playerDown = true;
// 			playerMoving = true;
// 		}
//
// 		if (Raylib.IsKeyDown(KeyboardKey.KEY_D) || Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT))
// 		{
// 			playerDir = 3;
// 			playerRight = true;
// 			playerMoving = true;
// 		}
// 	}
//
// 	private static void Update()
// 	{
// 		frameCount++;
//
// 		if (playerMoving)
// 		{
// 			if (playerUp)
// 				playerDst.y -= playerSpeed;
//
// 			if (playerLeft)
// 				playerDst.x -= playerSpeed;
//
// 			if (playerDown)
// 				playerDst.y += playerSpeed;
//
// 			if (playerRight)
// 				playerDst.x += playerSpeed;
//
// 			if (frameCount % 8 == 1)
// 				playerFrame = (playerFrame + 1) % 4;
// 		}
// 		else if (frameCount % 45 == 1)
// 		{
// 			// Idle animation
// 			playerFrame = (playerFrame + 1) % 2;
// 		}
//
// 		playerSrc.x = playerSrc.Width * playerFrame;
// 		playerSrc.y = playerSrc.Height * playerDir;
//
// 		camera.Target = new Vector2(playerDst.x - playerDst.Width / 2f, playerDst.y - playerDst.Height / 2f);
//
// 		playerMoving = false;
// 		playerUp = playerDown = playerRight = playerLeft = false;
// 	}
//
// 	private static void Render()
// 	{
// 		Raylib.BeginDrawing();
// 		Raylib.ClearBackground(new Color(147, 211, 196, 255));
// 		Raylib.BeginMode2D(camera);
// 		DrawScene();
// 		Raylib.EndMode2D();
// 		FpsDisplay.Update();
// 		Raylib.EndDrawing();
// 	}
//
// 	private static void DrawScene()
// 	{
// 		// Raylib.DrawTexture(grassSprite, 100, 50, Color.WHITE);
// 		for (int i = 0; i < tilemap.Count; i++)
// 		{
// 			if (tilemap[i] == 0)
// 				continue;
//
// 			tileDest.x = tileDest.Width * (i % mapW);
// 			tileDest.y = tileDest.Height * (i / mapW);
// 			tileSrc.x = tileSrc.Width * ((tilemap[i] - 1) % (grassSprite.Width / (int)tileSrc.Width));
// 			tileSrc.y = tileSrc.Height * ((tilemap[i] - 1) / (grassSprite.Width / (int)tileSrc.Width));
// 			Raylib.DrawTexturePro(grassSprite, tileSrc, tileDest, new Vector2(tileDest.Width, tileDest.Height), 0f,
// 				Color.WHITE);
// 		}
//
// 		Raylib.DrawTexturePro(playerSprite, playerSrc, playerDst, new Vector2(playerDst.Width, playerDst.Height), 0f,
// 			Color.WHITE);
// 	}
// }