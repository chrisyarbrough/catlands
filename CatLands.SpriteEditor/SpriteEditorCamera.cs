using System.Numerics;
using CatLands;
using CatLands.SpriteEditor;
using ImGuiNET;
using Raylib_cs;

public class SpriteEditorCamera : CameraController
{
	private readonly Func<Vector2> getTextureSize;

	public SpriteEditorCamera(Func<Vector2> getTextureSize)
	{
		this.getTextureSize = getTextureSize;
		SettingsWindow.Add("Camera", DrawSettings);
	}

	public override void Reset()
	{
		Vector2 size = getTextureSize();
		if (size.X != 0 && size.Y != 0)
		{
			// When zooming the camera to fit the texture into the viewport, account for the menubar and some extra space.
			const int menuBarHeight = 19;
			const int sizeOffset = menuBarHeight + 41;

			float zoomWidth = Raylib.GetRenderWidth() / size.X;
			float zoomHeight = (Raylib.GetRenderHeight() - sizeOffset) / size.Y;
			Camera.Zoom = MathF.Min(zoomWidth, zoomHeight);
			Camera.Target = new Vector2(size.X / 2f, size.Y / 2f);
			Camera.Offset = new Vector2(
				(Raylib.GetRenderWidth() / 2f),
				(Raylib.GetRenderHeight() - sizeOffset) / 2f + sizeOffset / 2f);
		}
	}

	public void DrawSettings()
	{
		bool changed1 = ImGui.DragFloat("Min", ref CameraMinZoom, 0.05f, 0.01f, 1f);
		bool changed2 = ImGui.DragFloat("Max", ref CameraMaxZoom, 0.05f, 1f, 100f);
		if (changed1 || changed2)
			Zoom(0f);

		if (ImGui.Button("Reset (R)"))
			Reset();
	}
}