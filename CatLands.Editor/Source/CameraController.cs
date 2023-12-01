namespace CatLands.Editor;

using System.Numerics;
using ImGuiNET;
using Raylib_cs;

public class CameraController
{
	private readonly SceneView sceneView;
	private bool hasHotControl;

	public CameraController(SceneView sceneView)
	{
		this.sceneView = sceneView;
	}

	public Camera2D Update(Camera2D camera)
	{
		if (Raylib.IsKeyPressed(KeyboardKey.KEY_C))
		{
			Reset(camera);
		}

		if ((Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT) ||
		     Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_MIDDLE)) &&
		    ImGui.IsWindowHovered())
		{
			hasHotControl = true;
		}

		if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_RIGHT) ||
		    Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_MIDDLE))
		{
			hasHotControl = false;
		}

		if (hasHotControl)
		{
			Vector2 delta = Raylib.GetMouseDelta();
			delta *= -1.0f / camera.Zoom;
			camera.Target += delta;
			sceneView.Repaint();
		}

		if (ImGui.IsWindowHovered() && Math.Abs(Raylib.GetMouseWheelMove()) > 0)
		{
			Vector2 mouseWorldPos = sceneView.GetMouseWorldPosition();

			camera.Offset = sceneView.GetMouseScreenPosition();

			// Set the target to match, so that the camera maps the world space point 
			// under the cursor to the screen space point under the cursor at any zoom
			camera.Target = mouseWorldPos;

			const float zoomSpeed = 0.125f;
			float zoomFactor = (float)Math.Log(camera.Zoom + 1, 10) * zoomSpeed;
			camera.Zoom = Math.Clamp(camera.Zoom + Raylib.GetMouseWheelMove() * zoomFactor, 0.01f, 10f);
			sceneView.Repaint();
		}

		return camera;
	}

	public Camera2D Reset(Camera2D camera)
	{
		camera.Target = Vector2.Zero;
		camera.Offset = sceneView.ViewportCenter();
		camera.Zoom = 1f;
		return camera;
	}
}