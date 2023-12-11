namespace CatLands.Editor;

using System.Numerics;
using ImGuiNET;
using Raylib_cs;

public class LayersWindow : Window
{
	private IntPtr hiddenImage;
	private IntPtr visibleImage;
	private int selectedLayerIndex;

	public LayersWindow() : base("Layers")
	{
	}

	public override void Setup()
	{
		hiddenImage = LoadTexture("hidden.png");
		visibleImage = LoadTexture("visible.png");
	}

	private static IntPtr LoadTexture(string fileName)
	{
		string directory = AppContext.BaseDirectory + "/EditorAssets/";
		return new IntPtr(Raylib.LoadTexture(directory + fileName).Id);
	}

	protected override void DrawContent()
	{
		Map? map = Map.Current;

		if (map == null)
			return;

		if (ImGui.Checkbox("Enable Grid", ref SceneView.EnableGrid))
			SceneView.RepaintAll();

		for (int i = 0; i < map.LayerCount; i++)
		{
			Layer layer = map.GetLayer(i);
			DrawLayer(layer, i);
		}
	}

	private void DrawLayer(Layer layer, int i)
	{
		ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));

		// if (ImGui.ImageButton(layer.Name, layer.IsVisible ? visibleImage : hiddenImage, new Vector2(16, 16)))
		if (ImGui.Button(layer.Name, new Vector2(16, 16)))
		{
			layer.IsVisible = !layer.IsVisible;
		}
		
		ImGui.PopStyleColor();
		
		// TODO
		// ImGui.SameLine();
		
		// Vector2 textSize = ImGui.CalcTextSize(layer.Name);
		// float textHeight = textSize.Y;
		// float verticalOffset = (20 - textHeight) * 0.5f;
		// float cursorPosY = ImGui.GetCursorPosY();
		// ImGui.SetCursorPosY(cursorPosY + verticalOffset);

		if (ImGui.RadioButton(layer.Name, ref selectedLayerIndex, i))
		{
			// This is never executed.
			Console.WriteLine("Clicked: " + i);
		}
	}
}