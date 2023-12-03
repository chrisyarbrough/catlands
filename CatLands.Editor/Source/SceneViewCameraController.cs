namespace CatLands.Editor;

using System.Numerics;
using Raylib_cs;

public class SceneViewCameraController : CameraController
{
	private readonly SceneView sceneView;

	public SceneViewCameraController(SceneView sceneView)
	{
		this.sceneView = sceneView;
	}

	public override void Reset()
	{
		Camera.Target = Vector2.Zero;
		Camera.Offset = sceneView.ViewportCenter();
		Camera.Zoom = 1f;
	}

	protected override Vector2 GetMouseWorld(Camera2D camera)
	{
		return sceneView.GetMouseWorldPosition();
	}

	protected override Vector2 GetMouseScreenPosition()
	{
		return sceneView.GetMouseScreenPosition();
	}

	protected override void OnChanged()
	{
		sceneView.Repaint();
	}
}