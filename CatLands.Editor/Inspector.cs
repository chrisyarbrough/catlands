namespace CatLands.Editor;

using ImGuiNET;

public class Inspector : Window
{
	private bool isOpen = true;

	public Inspector() : base("Inspector")
	{
	}

	public override void Render()
	{
		if (isOpen == false)
			return;
		ImGui.Begin(Name, ref isOpen);
		if (Selection.Current != null)
		{
			ImGui.SetNextItemOpen(true, ImGuiCond.Once);
			if (ImGui.CollapsingHeader(Selection.Current.Name))
			{
				ImGui.LabelText(Selection.Current.Children.Count().ToString(), "Child Count");
			}
		}
		ImGui.End();
	}
}