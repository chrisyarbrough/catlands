namespace CatLands.Editor;

using ImGuiNET;

public class Inspector : Window
{
	public Inspector() : base("Inspector")
	{
	}

	protected override void DrawContent()
	{
		if (Selection.Current != null)
		{
			ImGui.SetNextItemOpen(true, ImGuiCond.Once);
			if (ImGui.CollapsingHeader(Selection.Current.Name))
			{
				ImGui.LabelText(Selection.Current.Children.Count().ToString(), "Child Count");
			}
		}
	}
}