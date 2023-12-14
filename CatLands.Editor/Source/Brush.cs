namespace CatLands.Editor;

using System.Numerics;
using ImGuiNET;

public abstract class Brush
{
	public abstract string DisplayName { get; }

	public virtual void Initialize()
	{
	}

	public virtual void OnLayersChanged()
	{
	}

	public virtual void Update(SpriteAtlas atlas, int layerIndex, bool mouseOverWindow)
	{
	}

	public virtual void DrawUI(SpriteAtlas atlas, string textureId)
	{
	}

	protected static void DrawRect(Vector2 min, Vector2 max, Vector4 color)
	{
		ImGui.GetWindowDrawList().AddRect(
			min + Vector2.One * 0.5f,
			max - Vector2.One * 0.5f,
			ImGui.ColorConvertFloat4ToU32(color),
			rounding: 0,
			ImDrawFlags.None,
			thickness: 2);
	}
}