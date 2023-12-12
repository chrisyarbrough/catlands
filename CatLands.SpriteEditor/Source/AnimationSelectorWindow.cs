using System.Numerics;
using CatLands.SpriteEditor;
using ImGuiNET;

internal class AnimationSelectorWindow
{
	private static int selectedAnimationIndex;

	private const ImGuiTableFlags tableFlags =
		ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders;

	public static void Draw(SpriteAtlas spriteAtlas)
	{
		ImGui.ShowDemoWindow();
		DrawAnimationsSelectionWindow(spriteAtlas);
		AnimationEditorWindow.Draw(spriteAtlas, selectedAnimationIndex);
		AnimationPreviewWindow.Draw(spriteAtlas, selectedAnimationIndex);
	}

	private static void DrawAnimationsSelectionWindow(SpriteAtlas spriteAtlas)
	{
		if (ImGui.Begin("Animations"))
		{
			if (ImGui.Button("Create New"))
			{
				spriteAtlas.Add(new Animation()
				{
					Name = "New Animation",
					Frames = Selection.GetSelection()
						.Select(tileId => new Animation.Frame(tileId, 0.25f)).ToList(),
				});
				SaveDirtyTracker.MarkDirty();
			}

			ImGui.SameLine();

			HelpMarker(
				"Select a range of tiles on the loaded sheet before creating a new animation to automatically add them as frames.");

			ImGui.BeginTable("Animations", 4, tableFlags);
			ImGui.TableSetupColumn("Id", ImGuiTableColumnFlags.WidthFixed, 40);
			ImGui.TableSetupColumn("Name");
			ImGui.TableSetupColumn("Frame Count");
			ImGui.TableSetupColumn("Duration");
			ImGui.TableHeadersRow();

			for (int i = 0; i < spriteAtlas.Animations.Count; i++)
			{
				Animation animation = spriteAtlas.Animations[i];
				ImGui.TableNextRow();

				ImGui.TableSetColumnIndex(0);
				if (ImGui.Selectable(
					    i.ToString(),
					    selectedAnimationIndex == i,
					    ImGuiSelectableFlags.SpanAllColumns,
					    new Vector2(0, 17)))
				{
					selectedAnimationIndex = i;
				}

				ImGui.TableSetColumnIndex(1);
				ImGui.Text(animation.Name);

				ImGui.TableSetColumnIndex(2);
				ImGui.Text(animation.Frames.Count.ToString());

				ImGui.TableSetColumnIndex(3);
				ImGui.Text($"{animation.Duration.TotalSeconds:F2}s");
			}

			ImGui.EndTable();
		}

		ImGui.End();
	}

	private static void HelpMarker(string text)
	{
		ImGui.TextDisabled("(?)");
		if (ImGui.BeginItemTooltip())
		{
			ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
			ImGui.TextUnformatted(text);
			ImGui.PopTextWrapPos();
			ImGui.EndTooltip();
		}
	}
}