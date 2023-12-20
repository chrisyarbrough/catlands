namespace CatLands;

using System.Numerics;
using Raylib_cs;

public class CameraController
{
	public float CameraMinZoom = 0.5f;
	public float CameraMaxZoom = 20f;

	protected Camera2D Camera = new(Vector2.Zero, Vector2.Zero, rotation: 0f, zoom: 1f);

	private bool hasHotControl;

	private static readonly IInputAction panInputAction = MouseButtonAction.Pan;

	/// <summary>
	/// Returns a readonly copy of the camera state.
	/// </summary>
	public Camera2D State => Camera;

	public virtual void Reset()
	{
		Camera.Target = Vector2.Zero;
		Camera.Offset = Vector2.Zero;
		Camera.Zoom = 1f;
	}

	public void Begin()
	{
		Raylib.BeginMode2D(Camera);
	}

	public void Update(bool canBeginInputAction)
	{
		if (Raylib.IsKeyPressed(KeyboardKey.KEY_R))
		{
			Reset();
		}

		if (canBeginInputAction && panInputAction.IsStarted())
		{
			hasHotControl = true;
		}

		if (panInputAction.IsEnded())
		{
			hasHotControl = false;
		}

		if (hasHotControl)
		{
			Vector2 delta = Raylib.GetMouseDelta();
			Pan(delta);
		}

		if (canBeginInputAction && Math.Abs(Raylib.GetMouseWheelMove()) > 0)
		{
			Vector2 mouseWorldPos = GetMouseWorld(Camera);

			Camera.Offset = GetMouseScreenPosition();

			// Set the target to match, so that the camera maps the world space point 
			// under the cursor to the screen space point under the cursor at any zoom
			Camera.Target = mouseWorldPos;

			ApplyZoomDelta(Raylib.GetMouseWheelMove());
			OnChanged();
		}
	}

	private void Pan(Vector2 delta)
	{
		delta *= -1.0f / Camera.Zoom;

		if (delta.LengthSquared() > 0f)
		{
			Camera.Target += delta;
			OnChanged();
		}
	}

	public void ApplyZoomDelta(float delta)
	{
		const float zoomSpeed = 0.125f;
		float zoomFactor = (float)Math.Log(Camera.Zoom + 1, 10) * zoomSpeed;
		SetZoom(Camera.Zoom + delta * zoomFactor);
	}

	public void SetZoom(float zoom)
	{
		Camera.Zoom = Math.Clamp(
			zoom,
			CameraMinZoom,
			CameraMaxZoom);
	}

	public void End()
	{
		Raylib.EndMode2D();
	}

	/// <summary>
	/// A rectangle in world space describing the area that is visible.
	/// </summary>
	public Rectangle GetWorldBounds()
	{
		Vector2 topLeft = Raylib.GetScreenToWorld2D(
			new Vector2(0, 0),
			Camera);

		Vector2 bottomRight = Raylib.GetScreenToWorld2D(
			new Vector2(Raylib.GetRenderWidth(), Raylib.GetRenderHeight()),
			Camera);

		return new Rectangle(
			topLeft.X,
			topLeft.Y,
			bottomRight.X - topLeft.X,
			bottomRight.Y - topLeft.Y);
	}

	protected virtual Vector2 GetMouseWorld(Camera2D camera)
	{
		return Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
	}

	protected virtual Vector2 GetMouseScreenPosition()
	{
		return Raylib.GetMousePosition();
	}

	protected virtual void OnChanged()
	{
	}
}