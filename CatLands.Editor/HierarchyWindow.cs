namespace CatLands.Editor;

using ImGuiNET;

public class HierarchyWindow : Window
{
	private readonly Scene scene;

	public HierarchyWindow(Scene scene) : base("Hierarchy")
	{
		this.scene = scene;
	}

	public override void Render()
	{
		ImGui.Begin(Name, ImGuiWindowFlags.UnsavedDocument);
		DrawTreeNode(scene);
		ImGui.End();
	}

	private static void DrawTreeNode(GameObject node)
	{
		// Make the item expandable by clicking the arrow and prevent it from opening when clicking the label.
		var flags = ImGuiTreeNodeFlags.OpenOnArrow;

		// Do not draw the arrow for leaf nodes.
		if (!node.HasChildren)
			flags |= ImGuiTreeNodeFlags.Leaf;

		if (Selection.Current == node)
			flags |= ImGuiTreeNodeFlags.Selected;

		bool isExpanded = ImGui.TreeNodeEx(node.Name, flags);

		if (ImGui.IsItemClicked())
			Selection.Current = node;

		if (isExpanded)
		{
			foreach (GameObject child in node.Children)
				DrawTreeNode(child);

			ImGui.TreePop();
		}
	}
}