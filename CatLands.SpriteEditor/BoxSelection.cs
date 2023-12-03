using System.Numerics;
using CatLands.SpriteEditor;
using ImGuiNET;
using Raylib_cs;

public static class BoxSelection
{
	private const MouseButton mouseButton = MouseButton.MOUSE_BUTTON_LEFT;

	private static Vector2 startPosition;

	private static Shader shader;
	private static bool initialized;
	private static int timeShaderLocation;

	public static void Draw(Camera2D camera, int controlId, Action<Rectangle> onSelection)
	{
		if (GuiUtility.HotControl != -1 && GuiUtility.HotControl != controlId)
			return;
		
		// Disable input if hovering over a window.
		if (ImGui.GetIO().WantCaptureMouse)
			return;
		
		InitializeShader();

		Vector2 mouseWorld = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);

		if (Raylib.IsMouseButtonPressed(mouseButton))
		{
			startPosition = mouseWorld;
			GuiUtility.HotControl = controlId;
		}
		else if (Raylib.IsMouseButtonDown(mouseButton))
		{
			Rectangle rectangle = CalculateRenderRect(startPosition, mouseWorld);
			Raylib.SetShaderValue(
				shader, timeShaderLocation, (float)Raylib.GetTime(), ShaderUniformDataType.SHADER_UNIFORM_FLOAT);

			Raylib.BeginShaderMode(shader);
			const float lineWidth = 1f;
			float scaleFactor = 1.0f / camera.Zoom;
			Raylib.DrawRectangleLinesEx(rectangle, lineWidth * scaleFactor, Color.RED);
			Raylib.EndShaderMode();
		}
		else if (Raylib.IsMouseButtonReleased(mouseButton) && GuiUtility.HotControl == controlId)
		{
			Rectangle rectangle = CalculateRenderRect(startPosition, mouseWorld);
			onSelection.Invoke(rectangle);
			GuiUtility.HotControl = -1;
		}
	}

	private static void InitializeShader()
	{
		if (initialized == false)
		{
			shader = Raylib.LoadShader(null, "MarchingAnts.glsl");
			timeShaderLocation = Raylib.GetShaderLocation(shader, "TimeInSeconds");
			initialized = true;
		}
	}

	private static Rectangle CalculateRenderRect(Vector2 startPosition, Vector2 mousePosition)
	{
		// Create rectangle with positive width and height because Raylib doesn't draw inverted rects.
		float minX = Math.Min(startPosition.X, mousePosition.X);
		float minY = Math.Min(startPosition.Y, mousePosition.Y);
		float maxX = Math.Max(startPosition.X, mousePosition.X);
		float maxY = Math.Max(startPosition.Y, mousePosition.Y);

		return new Rectangle(
			minX,
			minY,
			maxX - minX,
			maxY - minY
		);
	}
}