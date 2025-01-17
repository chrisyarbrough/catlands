using System.Numerics;
using CatLands;
using CatLands.SpriteEditor;
using ImGuiNET;
using Raylib_cs;

public static class BoxSelect
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
		
		InitializeShader();

		Vector2 mouseWorld = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);

		// Only take hot control if clicking on the render scene, but not when clicking overlay windows.
		// Still allow releasing the mouse and finishing the selection, e.g. on the menu bar because it feels more responsive.
		if (Raylib.IsMouseButtonPressed(mouseButton) && !ImGui.GetIO().WantCaptureMouse)
		{
			startPosition = mouseWorld;
			GuiUtility.HotControl = controlId;
		}
		else if (Raylib.IsMouseButtonDown(mouseButton) && GuiUtility.HotControl == controlId)
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
			shader = Raylib.LoadShader(null, "Shaders/MarchingAnts.glsl");
			timeShaderLocation = Raylib.GetShaderLocation(shader, "TimeInSeconds");
			initialized = true;
		}
	}

	private static Rectangle CalculateRenderRect(Vector2 startPosition, Vector2 mousePosition)
	{
		// Create rectangle with positive width and height because Raylib doesn't draw inverted rects.
		return RectangleExtensions.FromPoints(startPosition, mousePosition);
	}
}