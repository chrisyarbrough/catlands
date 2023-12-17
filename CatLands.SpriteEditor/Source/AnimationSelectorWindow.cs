namespace CatLands.SpriteEditor;

using System.Numerics;
using ImGuiNET;
using Raylib_cs;

internal class AnimationSelectorWindow : Window
{
	private readonly AnimationEditor data;

	private const ImGuiTableFlags tableFlags =
		ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders;

	public AnimationSelectorWindow(AnimationEditor data) : base("Animations")
	{
		this.data = data;
	}

	protected override void DrawContent()
	{
		if (ImGui.Button("Create New (N)") || Raylib.IsKeyPressed(KeyboardKey.KEY_N))
		{
			AddNewAnimation();
		}

		ImGui.SameLine();

		ImGuiUtil.HelpMarker(
			"Select a range of tiles on the loaded sheet before creating a new animation to automatically add them as frames.");

		ImGui.BeginDisabled(data.SelectedAnimationIndex == -1);
		if (ImGui.Button("Delete"))
		{
			data.RemoveSelectedAnimation();
		}

		ImGui.EndDisabled();

		if (ImGui.BeginTable("Animations", 4, tableFlags))
		{
			ImGui.TableSetupColumn("Id", ImGuiTableColumnFlags.WidthFixed, 40);
			ImGui.TableSetupColumn("Name");
			ImGui.TableSetupColumn("Frame Count");
			ImGui.TableSetupColumn("Duration");
			ImGui.TableHeadersRow();

			for (int i = 0; i < data.Animations.Count; i++)
			{
				Animation animation = data.Animations[i];
				ImGui.TableNextRow();

				ImGui.TableSetColumnIndex(0);
				if (ImGui.Selectable(
					    i.ToString(),
					    data.SelectedAnimationIndex == i,
					    ImGuiSelectableFlags.SpanAllColumns,
					    new Vector2(0, 17)))
				{
					data.SelectedAnimationIndex = i;
					if (data.TryGetSelectedAnimation(out Animation a))
					{
						TileSelection.ClearSelection();
						for (int j = 0; j < a.FrameCount; j++)
						{
							TileSelection.AddToSelection(a.FrameAt(j).TileId);
						}
					}
				}

				ImGui.TableSetColumnIndex(1);
				ImGui.Text(animation.Name);

				ImGui.TableSetColumnIndex(2);
				ImGui.Text(animation.Frames.Count.ToString());

				ImGui.TableSetColumnIndex(3);
				ImGui.Text($"{animation.Duration:F2}s");
			}

			ImGui.EndTable();
		}
	}

	private void AddNewAnimation()
	{
		data.AddAnimation(new Animation()
		{
			Name = "New Anim " + data.Animations.Count,
			Frames = TileSelection.GetSelection()
				.Select(tileId => new Animation.Frame(tileId, 0.25f)).ToList(),
		});
	}
}