namespace CatLands.Editor;

using ImGuiNET;
using Raylib_cs;
using System.Numerics;

public static class CameraWidget
{
	private static readonly IntPtr iconPtr;
	private static readonly Vector2 size = new(44, 44);
	private static readonly int[] zoomLevels = { 25, 50, 75, 100, 150, 200 };

	static CameraWidget()
	{
		Texture2D iconTexture = Raylib.LoadTexture(
			"../CatLands.Editor/EditorAssets/camera-icon.png");

		Raylib.SetTextureFilter(iconTexture, TextureFilter.TEXTURE_FILTER_BILINEAR);
		iconPtr = new IntPtr(iconTexture.Id);
	}

	public static void Draw(SceneView sceneView)
	{
		ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
		ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));

		// Since there's no way to force a window to draw on top of others,
		// this widget relies on NoBringToFrontOnFocus being set on the scene view.
		Vector2 nextWindowPos = ImGui.GetWindowPos() + new Vector2(ImGui.GetWindowWidth() - size.X - 7, 45);
		ImGui.SetNextWindowPos(nextWindowPos);
		ImGui.SetNextWindowSizeConstraints(size, new Vector2(float.MaxValue, float.MaxValue));
		ImGui.SetNextWindowContentSize(size);
		ImGui.SetNextWindowSize(size);

		ImGui.Begin("Floating", ImGuiWindowFlags.NoDecoration |
		                        ImGuiWindowFlags.NoScrollbar |
		                        ImGuiWindowFlags.NoResize |
		                        ImGuiWindowFlags.NoBackground);
		if (ImGui.ImageButton("Camera", iconPtr, size))
		{
			ImGui.OpenPopup("ZoomLevel");
		}

		ImGui.PopStyleVar();
		ImGui.PopStyleVar();

		if (ImGui.BeginPopup("ZoomLevel"))
		{
			foreach (int zoomLevel in zoomLevels)
			{
				if (ImGui.Button(zoomLevel + "%", new Vector2(50, 25)))
				{
					sceneView.CameraZoom = zoomLevel / 100f;
					ImGui.CloseCurrentPopup();
				}
			}

			if (ImGui.Button("Reset All"))
			{
				sceneView.ResetCamera();
				ImGui.CloseCurrentPopup();
			}
		}

		ImGui.End();
	}
}