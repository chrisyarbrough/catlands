namespace CatLands.Editor;

using System.Numerics;
using Raylib_cs;

public class SceneView
{
	private Camera2D camera = new(Vector2.Zero, Vector2.Zero, rotation: 0f, zoom: 1f);

	public Vector2 GetMouseWorldPosition()
	{
		return Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
	}

	public SceneView()
	{
		RecenterToOrigin();
	}

	public void Update()
	{
		if (Raylib.IsKeyPressed(KeyboardKey.KEY_C))
		{
			RecenterToOrigin();
		}

		if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT) ||
		    Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_MIDDLE))
		{
			Vector2 delta = Raylib.GetMouseDelta();
			delta *= -1.0f / camera.zoom;
			camera.target += delta;
		}

		float wheel = Raylib.GetMouseWheelMove();

		if (wheel != 0)
		{
			Vector2 mouseWorldPos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);

			camera.offset = Raylib.GetMousePosition();

			// Set the target to match, so that the camera maps the world space point 
			// under the cursor to the screen space point under the cursor at any zoom
			camera.target = mouseWorldPos;

			const float zoomSpeed = 0.1f;
			float zoomFactor = MathF.Log(camera.zoom + 1) * zoomSpeed;
			camera.zoom += MathF.Sign(wheel) * zoomFactor;
			camera.zoom = Raymath.Clamp(camera.zoom, 0.1f, 10f);
		}
	}

	private void RecenterToOrigin()
	{
		camera.target = Vector2.Zero;
		camera.offset = new Vector2((Raylib.GetScreenWidth() - 200) / 2f + 200, Raylib.GetScreenHeight() / 2f);
		camera.zoom = 1f;
	}

	public void Begin()
	{
		Raylib.BeginMode2D(camera);
	}

	public void End()
	{
		Raylib.EndMode2D();
	}
}