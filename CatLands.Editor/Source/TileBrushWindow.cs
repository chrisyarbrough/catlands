namespace CatLands.Editor;

using ImGuiNET;
using NativeFileDialogSharp;

public class TileBrushWindow : Window
{
	private int layerIndex = -1;

	private readonly Brush[] brushes =
	{
		new SingleTileBrush(),
		new MultiTileBrush()
	};

	private int brushIndex;

	private Brush CurrentBrush => brushes[brushIndex];

	public TileBrushWindow() : base("Tile Brush")
	{
	}

	public override void Setup()
	{
		layerIndex = Prefs.Get("TileBrushWindow.LayerIndex", defaultValue: layerIndex);
		brushIndex = Prefs.Get("TileBrushWindow.BrushIndex", defaultValue: brushIndex);

		foreach (Brush brush in brushes)
			brush.Initialize();

		Map.CurrentChanged += OnCurrentChanged;
	}

	public override void Shutdown()
	{
		Map.CurrentChanged -= OnCurrentChanged;
	}

	private void OnCurrentChanged()
	{
		if (Map.Current != null && Map.Current.LayerCount > 0)
			layerIndex = 0;
		else
			layerIndex = -1;

		foreach (Brush brush in brushes)
			brush.OnLayersChanged();
	}

	public override void Update()
	{
		if (SceneView.Current == null || Map.Current == null || Map.Current.LayerCount == 0)
			return;

		if (MapTextures.TextureCount <= layerIndex || layerIndex == -1)
			return;

		string textureId = Map.Current.GetLayer(layerIndex).TexturePath;
		SpriteAtlas atlas = MapTextures.GetAtlas(textureId);
		bool mouseOverWindow = SceneView.Current.IsMouseOverWindow;

		CurrentBrush.Update(atlas, layerIndex, mouseOverWindow);

		layerIndex = Math.Clamp(layerIndex, 0, Map.Current.LayerCount - 1);
	}

	protected override void DrawContent()
	{
		if (Map.Current == null)
			return;

		DrawLayerDropdown();

		MapTextures.UpdateLoadState();

		if (MapTextures.TextureCount <= layerIndex || layerIndex == -1)
			return;

		string textureId = Map.Current.GetLayer(layerIndex).TexturePath;
		SpriteAtlas atlas = MapTextures.GetAtlas(textureId);

		DrawToolSelection();

		CurrentBrush.DrawUI(atlas, textureId);
	}

	private void DrawToolSelection()
	{
		for (int i = 0; i < brushes.Length; i++)
		{
			if (ImGui.RadioButton(brushes[i].DisplayName, ref brushIndex, i))
				Prefs.Set("TileBrushWindow.BrushIndex", i);
		}
	}

	private void DrawLayerDropdown()
	{
		// TODO: draw layers one by one (maybe in separate layer window) to select it via single click.
		string[] options = Map.Current!.Layers.Select(x => x.Name).ToArray();
		if (ImGui.Combo("Layer", ref layerIndex, options, options.Length))
		{
			foreach (Brush brush in brushes)
				brush.Initialize();

			Prefs.Set("TileBrushWindow.LayerIndex", layerIndex);
		}

		ImGui.SameLine();

		if (ImGui.Button("Add"))
		{
			DialogResult result = Dialog.FileOpen(defaultPath: "Assets");
			if (result.IsOk)
			{
				var command = new AddLayerCommand(Map.Current, result.Path);
				CommandManager.Execute(command);
				layerIndex = command.AddedLayerId;
				foreach (Brush brush in brushes)
					brush.OnLayersChanged();
			}
		}
	}
}