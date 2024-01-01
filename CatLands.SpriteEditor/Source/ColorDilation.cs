// using System.Numerics;
// using CatLands;
// using CatLands.SpriteEditor;
// using ImGuiNET;
// using Newtonsoft.Json;
// using Raylib_cs;
// using RectpackSharp;
// using File = System.IO.File;
//
// public class ColorDilation
// {
// 	private Shader shader;
// 	private Shader alphaMaskShader;
// 	private bool initialized;
//
// 	private bool useShader = true;
// 	private bool usePacking = true;
// 	private RenderTexture2D target;
// 	private RenderTexture2D target2;
// 	private bool targetInit;
// 	private const int padding = 2;
//
// 	public void Draw(SpriteAtlas spriteAtlas)
// 	{
// 		DrawPreview(spriteAtlas);
//
// 		PackingRectangle[] rectangles = new PackingRectangle[spriteAtlas.Sprites.Count];
// 		for (int i = 0; i < spriteAtlas.Sprites.Count; i++)
// 		{
// 			Rect rect = spriteAtlas.Sprites[i].Inflate(padding);
// 			rectangles[i] = new PackingRectangle((uint)rect.X, (uint)rect.Y, (uint)rect.Width, (uint)rect.Height, i);
// 		}
//
// 		RectanglePacker.Pack(rectangles, out PackingRectangle bounds, PackingHints.MostlySquared);
//
// 		ImGui.Checkbox("Use Packing", ref usePacking);
//
// 		if (!targetInit)
// 		{
// 			targetInit = true;
// 			target = Raylib.LoadRenderTexture((int)bounds.Width, (int)bounds.Height);
// 			Raylib.SetTextureFilter(target.Texture, TextureFilter.TEXTURE_FILTER_POINT);
// 			target2 = Raylib.LoadRenderTexture((int)bounds.Width, (int)bounds.Height);
// 			Raylib.SetTextureFilter(target2.Texture, TextureFilter.TEXTURE_FILTER_POINT);
// 			Raylib.SetTextureWrap(target2.Texture, TextureWrap.TEXTURE_WRAP_CLAMP);
// 		}
//
// 		Raylib.BeginTextureMode(target);
// 		Raylib.ClearBackground(Color.BLANK);
//
// 		Raylib.BeginShaderMode(alphaMaskShader);
// 		List<Rectangle> newSpriteRects = new();
// 		for (int i = 0; i < rectangles.Length; i++)
// 		{
// 			PackingRectangle r = rectangles[i];
// 			Rectangle rect = new Rectangle(r.X, r.Y, r.Width, r.Height).GrowBy(-padding);
// 			newSpriteRects.Add(rect);
//
// 			Raylib.DrawTexturePro(spriteAtlas.Texture, spriteAtlas.Sprites[r.Id], rect, Vector2.Zero, 0f,
// 				Color.WHITE);
//
// 			//Raylib.DrawRectangle((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height, r.Id % 2 == 0 ? new Color(255, 0, 0, 20) : new Color(0, 255, 0, 20));
// 		}
// 		Raylib.EndShaderMode();
//
// 		//
// 		Raylib.EndTextureMode();
// 		Image alphaMask = Raylib.LoadImageFromTexture(target.Texture);
//
// 		Raylib.BeginTextureMode(target);
// 		Raylib.ClearBackground(Color.BLANK);
//
// 		for (int i = 0; i < rectangles.Length; i++)
// 		{
// 			PackingRectangle r = rectangles[i];
// 			Rectangle rect = new Rectangle(r.X, r.Y, r.Width, r.Height).GrowBy(-padding);
//
// 			Raylib.DrawTexturePro(spriteAtlas.Texture, spriteAtlas.Sprites[r.Id], rect, Vector2.Zero, 0f,
// 				Color.WHITE);
//
// 			//Raylib.DrawRectangle((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height, r.Id % 2 == 0 ? new Color(255, 0, 0, 20) : new Color(0, 255, 0, 20));
// 		}
//
// 		//
// 		Raylib.EndTextureMode();
//
// 		Raylib.BeginTextureMode(target2);
// 		Raylib.ClearBackground(Color.BLANK);
// 		if (useShader)
// 			Raylib.BeginShaderMode(shader);
// 		//
// 		Raylib.DrawTexturePro(target.Texture,
// 			new Rectangle(0, 0, target.Texture.Width, -target.Texture.Height),
// 			new Rectangle(0, 0, target.Texture.Width, target.Texture.Height),
// 			Vector2.Zero, 0f,Color.WHITE);
//
// 		if (useShader)
// 			Raylib.EndShaderMode();
// 		Raylib.EndTextureMode();
//
//
// 		Raylib.BeginTextureMode(target);
// 		Raylib.ClearBackground(Color.BLANK);
// 		if (useShader)
// 			Raylib.BeginShaderMode(shader);
// 		//
// 		Raylib.DrawTexturePro(target2.Texture,
// 			new Rectangle(0, 0, target.Texture.Width, -target.Texture.Height),
// 			new Rectangle(0, 0, target.Texture.Width, target.Texture.Height),
// 			Vector2.Zero, 0f,Color.WHITE);
//
// 		if (useShader)
// 			Raylib.EndShaderMode();
// 		Raylib.EndTextureMode();
//
//
// 		Raylib.DrawTextureEx(target.Texture, Vector2.Zero, 0f, scale: 6f, Color.WHITE);
//
//
// 		Image image = Raylib.LoadImageFromTexture(target.Texture);
// 		Raylib.ImageAlphaMask(ref image, alphaMask);
//
// 		Raylib.ExportImage(alphaMask, "MyFileA.png");
// 		Raylib.ExportImage(image, "MyFile.png");
//
// 		string json = JsonConvert.SerializeObject(newSpriteRects, Formatting.Indented);
// 		File.WriteAllText("MyFile.json", json);
// 	}
//
// 	private void DrawPreview(SpriteAtlas spriteAtlas)
// 	{
// 		if (!initialized)
// 		{
// 			shader = Raylib.LoadShader(null, "ColorDilate.glsl");
// 			alphaMaskShader = Raylib.LoadShader(null, "AlphaMask.glsl");
// 			initialized = true;
// 		}
//
// 		if (Raylib.IsKeyPressed(KeyboardKey.KEY_S))
// 			useShader = !useShader;
//
// 		ImGui.TextUnformatted("Use shader: " + useShader);
// 	}
// }